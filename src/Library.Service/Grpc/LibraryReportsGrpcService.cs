using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Library.Contracts.Reports;
using Library.Service.Reports;

namespace Library.Service.Grpc;

public sealed class LibraryReportsGrpcService(ILibraryReport reports, ILogger<LibraryReportsGrpcService> logger)
    : LibraryReportsRpc.LibraryReportsRpcBase
{
    public override async Task<MostBorrowedBooksResponse> GetMostBorrowedBooks(MostBorrowedBooksRequest request,
        ServerCallContext context)
    {
        try
        {
            var books = await reports
                .GetMostBorrowedBooksAsync(request.Limit,
                    context.CancellationToken);

            var response = new MostBorrowedBooksResponse();

            response.Books.AddRange(books.Select(b => new BorrowedBook
            {
                BookId = b.BookId,
                BorrowCount = b.BorrowCount,
                Title = b.Title
            }));

            return response;
        }
        catch (OperationCanceledException)
            when (context.CancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw CreateRpcException(exception);
        }
    }

    public override async Task<MostActiveBorrowersResponse> GetMostActiveBorrowers(MostActiveBorrowersRequest request,
        ServerCallContext context)
    {
        try
        {
            var from = ParseTimestamp(request.From, nameof(request.From));
            var to = ParseTimestamp(request.To, nameof(request.To));

            var reportResults =
                await reports.GetMostActiveBorrowerAsync(from, to, request.Limit, context.CancellationToken);
            var response = new MostActiveBorrowersResponse();

            response.Borrowers.AddRange(reportResults.Select(x => new ActiveBorrower
            {
                Name = x.Name,
                BorrowerId = x.BorrowerId,
                BorrowCount = x.BorrowCount
            }));

            return response;
        }
        catch (OperationCanceledException)
            when (context.CancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw CreateRpcException(exception);
        }
    }


    public override async Task<ReadingPaceResponse> GetReadingPace(ReadingPaceRequest request,
        ServerCallContext context)
    {
        try
        {
            if (request.LoanId < 1)
                throw new ArgumentException("Loan id must be greater than zero.", nameof(request.LoanId));

            var result = await reports.GetReadingPaceAsync(request.LoanId, context.CancellationToken);

            return new ReadingPaceResponse
            {
                LoanId = result.LoanId,
                BorrowerId = result.BorrowerId,
                BookId = result.BookId,
                BookTitle = result.BookTitle,
                BorrowerName = result.BorrowerName,
                PageCount = result.PageCount,
                ElapsedDays = result.ElapsedDays,
                PagesPerDay = result.PagesPerDay
            };
        }
        catch (OperationCanceledException)
            when (context.CancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw CreateRpcException(exception);
        }
    }

    public override async Task<ReadersAlsoBorrowedResponse> GetReadersAlsoBorrowed(
        ReadersAlsoBorrowedRequest request,
        ServerCallContext context)
    {
        try
        {
            var books = await reports.GetReadersAlsoBorrowedAsync(
                request.BookId,
                request.Limit,
                context.CancellationToken);

            var response = new ReadersAlsoBorrowedResponse();

            response.Books.AddRange(books.Select(b => new ReadersAlsoBorrowedBook
            {
                BookId = b.BookId,
                Title = b.Title,
                ReaderCount = b.ReaderCount
            }));

            return response;
        }
        catch (OperationCanceledException)
            when (context.CancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw CreateRpcException(exception);
        }
    }

    private RpcException CreateRpcException(Exception exception)
    {
        return exception switch
        {
            ArgumentException e => new RpcException(new Status(StatusCode.InvalidArgument, e.Message)),

            KeyNotFoundException e => new RpcException(new Status(StatusCode.NotFound, e.Message)),

            LoanNotReturnedException e => new RpcException(new Status(StatusCode.FailedPrecondition, e.Message)),

            LoanInvalidDurationException e => new RpcException(new Status(StatusCode.FailedPrecondition, e.Message)),

            LoanNotFoundException e => new RpcException(new Status(StatusCode.NotFound, e.Message)),

            _ => LogAndWrapUnexpected(exception)
        };
    }

    private RpcException LogAndWrapUnexpected(Exception exception)
    {
        logger.LogError(exception, "Unexpected error in gRPC call.");
        return new RpcException(new Status(StatusCode.Internal, "An unexpected error occurred."));
    }

    private static DateTimeOffset ParseTimestamp(
        Timestamp? timestamp,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(timestamp, parameterName);

        try
        {
            return timestamp.ToDateTimeOffset();
        }
        catch (InvalidOperationException exception)
        {
            throw new ArgumentException(
                "Invalid timestamp.",
                parameterName,
                exception);
        }
    }
}
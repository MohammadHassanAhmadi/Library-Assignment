using Google.Protobuf.WellKnownTypes;
using Library.Api.Contracts;
using Library.Contracts.Reports;

namespace Library.Api.Endpoints
{
    public static class ReportEndpoints
    {

        public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
        {
            var reports = app.MapGroup("/api/reports").WithTags("Reports");

            reports.MapGet("/most-borrowed-books", GetMostBorrowedBooks);
            reports.MapGet("/most-active-borrowers", GetMostActiveBorrowers);
            reports.MapGet("/loans/{loanId:int}/reading-pace", GetReadingPace);
            reports.MapGet("/books/{bookId:int}/readers-also-borrowed", GetReadersAlsoBorrowed);

            return app;
        }

        private static async Task<IResult> GetReadersAlsoBorrowed(LibraryReportsRpc.LibraryReportsRpcClient rpcClient, int bookId, CancellationToken cancellationToken, int limit = 10)
        {
            var response = await rpcClient.GetReadersAlsoBorrowedAsync(new ReadersAlsoBorrowedRequest
            {
                BookId = bookId,
                Limit = limit
            }, cancellationToken: cancellationToken);


            return Results.Ok(response.Books.Select(book => new ReadersAlsoBorrowedDto(book.BookId, book.Title, book.ReaderCount)));

        }

        private static async Task<IResult> GetReadingPace(LibraryReportsRpc.LibraryReportsRpcClient rpcClient,
            int loanId, CancellationToken cancellationToken)
        {
            var response = await rpcClient.GetReadingPaceAsync(new ReadingPaceRequest {LoanId = loanId},
                cancellationToken: cancellationToken);

            return Results.Ok(new ReadingPaceDto(response.LoanId,
                response.BorrowerId,
                response.BorrowerName,
                response.BookId,
                response.BookTitle,
                response.PageCount,
                response.ElapsedDays,
                response.PagesPerDay));
        }

        private static async Task<IResult> GetMostActiveBorrowers(LibraryReportsRpc.LibraryReportsRpcClient rpcClient,
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken cancellationToken,
            int limit = 10)
        {
            var response = await rpcClient.GetMostActiveBorrowersAsync(new MostActiveBorrowersRequest
                {
                    From = Timestamp.FromDateTimeOffset(from),
                    To = Timestamp.FromDateTimeOffset(to),
                    Limit = limit
                }, cancellationToken:cancellationToken);

            return Results.Ok(response.Borrowers.Select(borrower => new ActiveBorrowerDto(borrower.BorrowerId, borrower.Name, borrower.BorrowCount)));
        }

        private static async Task<IResult> GetMostBorrowedBooks(
            LibraryReportsRpc.LibraryReportsRpcClient rpcClient,
            CancellationToken cancellationToken,
            int limit = 10)
        {
            var response = await rpcClient.GetMostBorrowedBooksAsync(new MostBorrowedBooksRequest {Limit = limit},
                cancellationToken: cancellationToken);

            return Results.Ok(
                response.Books.Select(book => new BorrowedBookDto(book.BookId, book.Title, book.BorrowCount)));
        }
    }
}

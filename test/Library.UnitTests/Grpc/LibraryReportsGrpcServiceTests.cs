using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Library.Contracts.Reports;
using Library.Service.Grpc;
using Library.Service.Reports;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;


namespace Library.UnitTests.Grpc;

public class LibraryReportsGrpcServiceTests
{
    private readonly Mock<ILibraryReport> _reports = new();

    private LibraryReportsGrpcService CreateGrpcService()
    {
        return new LibraryReportsGrpcService(_reports.Object, NullLogger<LibraryReportsGrpcService>.Instance);
    }


    [Fact]
    public async Task GetMostBorrowedBooks_MapsEveryResultToResponse()
    {
        _reports
            .Setup(r => r.GetMostBorrowedBooksAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new BorrowedBookResults(1, "Moby Dick", 5),
                new BorrowedBookResults(2, "SQL Foundations", 3)
            ]);


        var response = await CreateGrpcService()
            .GetMostBorrowedBooks(new MostBorrowedBooksRequest {Limit = 2}, FakeServerCallContext.Create());

        Assert.Equal(2, response.Books.Count);

        Assert.Equal(1, response.Books[0].BookId);
        Assert.Equal("Moby Dick", response.Books[0].Title);
        Assert.Equal(5, response.Books[0].BorrowCount);

        Assert.Equal(2, response.Books[1].BookId);
        Assert.Equal("SQL Foundations", response.Books[1].Title);
        Assert.Equal(3, response.Books[1].BorrowCount);
    }

    [Fact]
    public async Task GetMostActiveBorrowers_ConvertsTimestampToDatetimeOffset()
    {
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

        _reports
            .Setup(r => r.GetMostActiveBorrowerAsync(from, to, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ActiveBorrowerResults(7, "Alice", 4)]);

        var response = await CreateGrpcService().GetMostActiveBorrowers(
            new MostActiveBorrowersRequest
            {
                From = Timestamp.FromDateTimeOffset(from),
                To = Timestamp.FromDateTimeOffset(to),
                Limit = 10
            }, FakeServerCallContext.Create());

        var borrower = Assert.Single(response.Borrowers);
        Assert.Equal(7, borrower.BorrowerId);
        Assert.Equal("Alice", borrower.Name);
        Assert.Equal(4, borrower.BorrowCount);
    }

    [Fact]
    public async Task GetMostActiveBorrowers_WhenFromIsMissing_ThrowInvalidArgument()
    {
        var to = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var grpcService = CreateGrpcService();
        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            grpcService.GetMostActiveBorrowers(new MostActiveBorrowersRequest
                {
                    To = Timestamp.FromDateTimeOffset(to),
                    Limit = 10
                }, FakeServerCallContext.Create()
            ));

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task GetReadingPace_MapsEveryField()
    {
        _reports
            .Setup(r => r.GetReadingPaceAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReadingPaceResult(3, 9, 4, "Moby Dick", "Alice", 500, 10d, 50d));

        var response = await CreateGrpcService()
            .GetReadingPace(new ReadingPaceRequest {LoanId = 3}, FakeServerCallContext.Create());

        Assert.Equal(3, response.LoanId);
        Assert.Equal(9, response.BorrowerId);
        Assert.Equal(4, response.BookId);
        Assert.Equal("Moby Dick", response.BookTitle);
        Assert.Equal("Alice", response.BorrowerName);
        Assert.Equal(500, response.PageCount);
        Assert.Equal(10d, response.ElapsedDays);
        Assert.Equal(50d, response.PagesPerDay);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetReadingPace_WhenLoanIdIsNotPositive_ThrowsInvalidArgument(int loanId)
    {
        var grpcService = CreateGrpcService();
        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            grpcService.GetReadingPace(new ReadingPaceRequest {LoanId = loanId}, FakeServerCallContext.Create()));


        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task GetReadingPace_WhenLoanDoesNotExist_ThrowsNotFound()
    {
        _reports.Setup(r => r.GetReadingPaceAsync(99, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LoanNotFoundException(99));

        var exception = await Assert.ThrowsAsync<RpcException>(() => CreateGrpcService()
            .GetReadingPace(new ReadingPaceRequest {LoanId = 99}, FakeServerCallContext.Create()));

        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task GetReadingPace_WhenLoanIsStillOut_ThrowsFailedPrecondition()
    {
        _reports
            .Setup(r => r.GetReadingPaceAsync(6, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LoanNotReturnedException(6));

        var exception = await Assert.ThrowsAsync<RpcException>(() => CreateGrpcService()
            .GetReadingPace(new ReadingPaceRequest {LoanId = 6}, FakeServerCallContext.Create()));

        Assert.Equal(StatusCode.FailedPrecondition, exception.StatusCode);
    }

    [Fact]
    public async Task GetReadersAlsoBorrowed_MapsEveryResultToResponse()
    {
        _reports
            .Setup(r => r.GetReadersAlsoBorrowedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ReadersAlsoBorrowedResult(3, "SQL Basics", 2)]);

        var response = await CreateGrpcService().GetReadersAlsoBorrowed(new ReadersAlsoBorrowedRequest
        {
            BookId = 1,
            Limit = 10
        }, FakeServerCallContext.Create());

        var book = Assert.Single(response.Books);
        Assert.Equal(3, book.BookId);
        Assert.Equal("SQL Basics", book.Title);
        Assert.Equal(2, book.ReaderCount);
    }


    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public async Task GetMostBorrowedBooks_WhenLimitIsRejected_ThrowsInvalidArgument(int limit)
    {
        _reports
            .Setup(r => r.GetMostBorrowedBooksAsync(limit, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentOutOfRangeException("limit", limit, "Limit must be between 1 and 100."));

        var exception = await Assert.ThrowsAsync<RpcException>(() => CreateGrpcService()
            .GetMostBorrowedBooks(new MostBorrowedBooksRequest {Limit = limit}, FakeServerCallContext.Create()));

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task GetMostBorrowedBooks_WhenUnexpectedErrorOccurs_ThrowsInternalWithoutLeakingDetail()
    {
        _reports.Setup(r => r.GetMostBorrowedBooksAsync(10, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Some Error leaking details here"));

        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            CreateGrpcService()
                .GetMostBorrowedBooks(new MostBorrowedBooksRequest {Limit = 10}, FakeServerCallContext.Create())
        );

        Assert.Equal(StatusCode.Internal, exception.StatusCode);
        Assert.DoesNotContain("Some Error leaking details here", exception.Status.Detail);
    }

    [Fact]
    public async Task GetMostBorrowedBooks_WhenClientCancels_PropagatesCancellationInsteadOfRpcException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _reports
            .Setup(r => r.GetMostBorrowedBooksAsync(5, It.Is<CancellationToken>(t => t.IsCancellationRequested)))
            .ThrowsAsync(new OperationCanceledException());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CreateGrpcService()
            .GetMostBorrowedBooks(
                new MostBorrowedBooksRequest {Limit = 5},
                FakeServerCallContext.Create(cts.Token)));

        _reports.VerifyAll();
    }

    [Fact]
    public async Task GetMostBorrowedBooks_WhenCancellationDidNotComeFromClient_ThrowsInternal()
    {
        _reports
            .Setup(r => r.GetMostBorrowedBooksAsync(5, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException("read query timed out"));

        var exception = await Assert.ThrowsAsync<RpcException>(() => CreateGrpcService()
            .GetMostBorrowedBooks(new MostBorrowedBooksRequest {Limit = 5}, FakeServerCallContext.Create()));

        Assert.Equal(StatusCode.Internal, exception.StatusCode);
        Assert.DoesNotContain("timed out", exception.Status.Detail);
    }

    [Fact]
    public async Task GetMostActiveBorrowers_WhenClientCancels_PropagatesCancellationInsteadOfRpcException()
    {
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _reports
            .Setup(r => r.GetMostActiveBorrowerAsync(from, to, 10,
                It.Is<CancellationToken>(t => t.IsCancellationRequested)))
            .ThrowsAsync(new OperationCanceledException());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CreateGrpcService()
            .GetMostActiveBorrowers(
                new MostActiveBorrowersRequest
                {
                    From = Timestamp.FromDateTimeOffset(from),
                    To = Timestamp.FromDateTimeOffset(to),
                    Limit = 10
                },
                FakeServerCallContext.Create(cts.Token)));

        _reports.VerifyAll();
    }


    [Fact]
    public async Task GetMostActiveBorrowers_WhenCancellationDidNotComeFromClient_ThrowsInternal()
    {
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

        _reports
            .Setup(r => r.GetMostActiveBorrowerAsync(from, to, 10, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException("read query timed out"));

        var exception = await Assert.ThrowsAsync<RpcException>(() => CreateGrpcService()
            .GetMostActiveBorrowers(
                new MostActiveBorrowersRequest
                {
                    From = Timestamp.FromDateTimeOffset(from),
                    To = Timestamp.FromDateTimeOffset(to),
                    Limit = 10
                },
                FakeServerCallContext.Create()));

        Assert.Equal(StatusCode.Internal, exception.StatusCode);
        Assert.DoesNotContain("timed out", exception.Status.Detail);
    }

    [Fact]
    public async Task GetReadingPace_WhenClientCancels_PropagatesCancellationInsteadOfRpcException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _reports
            .Setup(r => r.GetReadingPaceAsync(3, It.Is<CancellationToken>(t => t.IsCancellationRequested)))
            .ThrowsAsync(new OperationCanceledException());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CreateGrpcService()
            .GetReadingPace(
                new ReadingPaceRequest {LoanId = 3},
                FakeServerCallContext.Create(cts.Token)));

        _reports.VerifyAll();
    }

    [Fact]
    public async Task GetReadingPace_WhenCancellationDidNotComeFromClient_ThrowsInternal()
    {
        _reports
            .Setup(r => r.GetReadingPaceAsync(3, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException("read query timed out"));

        var exception = await Assert.ThrowsAsync<RpcException>(() => CreateGrpcService()
            .GetReadingPace(new ReadingPaceRequest {LoanId = 3}, FakeServerCallContext.Create()));

        Assert.Equal(StatusCode.Internal, exception.StatusCode);
        Assert.DoesNotContain("timed out", exception.Status.Detail);
    }

    [Fact]
    public async Task GetReadersAlsoBorrowed_WhenClientCancels_PropagatesCancellationInsteadOfRpcException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _reports
            .Setup(r => r.GetReadersAlsoBorrowedAsync(1, 10,
                It.Is<CancellationToken>(t => t.IsCancellationRequested)))
            .ThrowsAsync(new OperationCanceledException());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CreateGrpcService()
            .GetReadersAlsoBorrowed(
                new ReadersAlsoBorrowedRequest {BookId = 1, Limit = 10},
                FakeServerCallContext.Create(cts.Token)));

        _reports.VerifyAll();
    }

    [Fact]
    public async Task GetReadersAlsoBorrowed_WhenCancellationDidNotComeFromClient_ThrowsInternal()
    {
        _reports
            .Setup(r => r.GetReadersAlsoBorrowedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException("read query timed out"));

        var exception = await Assert.ThrowsAsync<RpcException>(() => CreateGrpcService()
            .GetReadersAlsoBorrowed(
                new ReadersAlsoBorrowedRequest {BookId = 1, Limit = 10},
                FakeServerCallContext.Create()));

        Assert.Equal(StatusCode.Internal, exception.StatusCode);
        Assert.DoesNotContain("timed out", exception.Status.Detail);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task GetReadersAlsoBorrowed_WhenLimitIsRejected_ThrowTsInvalidArgument(int limit)
    {
        _reports
            .Setup(r => r.GetReadersAlsoBorrowedAsync(1, limit, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentOutOfRangeException("limit", limit, "Limit must be between 1 and 100."));

        var exception = await Assert.ThrowsAsync<RpcException>(() => CreateGrpcService()
            .GetReadersAlsoBorrowed(new ReadersAlsoBorrowedRequest { BookId = 1, Limit = limit },
                FakeServerCallContext.Create()));

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task GetMostActiveBorrowers_WhenTimestampIsOutOfRange_ThrowsInvalidArgument()
    {
        var grpcService = CreateGrpcService();

        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            grpcService.GetMostActiveBorrowers(new MostActiveBorrowersRequest
            {
                From = new Timestamp { Seconds = long.MaxValue },
                To = Timestamp.FromDateTimeOffset(DateTimeOffset.UnixEpoch),
                Limit = 10
            }, FakeServerCallContext.Create()));

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task GetReadingPace_WhenDurationIsInvalid_ThrowsFailedPrecondition()
    {
        var reportException = new LoanInvalidDurationException(3);

        _reports
            .Setup(r => r.GetReadingPaceAsync(3, It.IsAny<CancellationToken>()))
            .ThrowsAsync(reportException);

        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            CreateGrpcService().GetReadingPace(
                new ReadingPaceRequest { LoanId = 3 },
                FakeServerCallContext.Create()));

        Assert.Equal(StatusCode.FailedPrecondition, exception.StatusCode);
        Assert.Equal(reportException.Message, exception.Status.Detail);
    }
}
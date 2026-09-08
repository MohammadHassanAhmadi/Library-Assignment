using Library.Service.Data;

namespace Library.IntegrationTests.Reports;

[Collection(LibraryDatabaseCollection.Name)]
public class LibraryReportsTests(LibraryDatabaseFixture fixture)
{
    private static readonly DateTimeOffset January = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset February = new(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset March = new(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetMostBorrowedBooksAsync_RanksBooksByBorrowCount()
    {
        await using var dbcontext = fixture.CreateDbContext();

        var books = await new LibraryReports(dbcontext).GetMostBorrowedBooksAsync(10);

        Assert.Collection(books,
            book => Assert.Equal(("Lord of the Rings - Fellowship of the Rings", 3), (book.Title, book.BorrowCount)),
            book => Assert.Equal(("SQL Basics", 2), (book.Title, book.BorrowCount)),
            book => Assert.Equal(("API Design", 1), (book.Title, book.BorrowCount)));
    }

    [Fact]
    public async Task GetMostBorrowedBooksAsync_RespectsLimit()
    {
        await using var dbContext = fixture.CreateDbContext();
        var books = await new LibraryReports(dbContext).GetMostBorrowedBooksAsync(1);

        Assert.Single(books);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task GetMostBorrowedBooksAsync_WhenLimitIsOutOfRange_ThrowsArgumentOutOfRangeException(int limit)
    {
        await using var dbContext = fixture.CreateDbContext();
        
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            new LibraryReports(dbContext).GetMostBorrowedBooksAsync(limit));
    }

    [Fact]
    public async Task GetMostActiveBorrowerAsync_CountsEveryBorrowerInTheWindow()
    {
        await using var dbContext = fixture.CreateDbContext();

        var borrowers = await new LibraryReports(dbContext).GetMostActiveBorrowerAsync(January, March, 10);

        Assert.Equal(3, borrowers.Count);
        Assert.All(borrowers, borrower => Assert.Equal(2, borrower.BorrowerCount));
    }

    [Fact]
    public async Task GetMostActiveBorrowerAsync_IncludesLoansThatWereNeverReturned()
    {
        await using var dbContext = fixture.CreateDbContext();

        var borrowers = await new LibraryReports(dbContext).GetMostActiveBorrowerAsync(February, March, 10);

        var carol = Assert.Single(borrowers);
        Assert.Equal("Carol", carol.Name);
        Assert.Equal(2, carol.BorrowerCount);

    }

    [Fact]
    public async Task GetMostActiveBorrowerAsync_WhenRangeIsInvalid_ThrowsArgumentException()
    {
        await using var dbContext = fixture.CreateDbContext();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            new LibraryReports(dbContext).GetMostActiveBorrowerAsync(March, January, 10));
    }

}
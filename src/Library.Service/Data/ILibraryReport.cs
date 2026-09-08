using Library.Service.Reports;

namespace Library.Service.Data;

public interface ILibraryReport
{
    Task<IReadOnlyList<BorrowedBookResults>> GetMostBorrowedBooksAsync(int limit,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ActiveBorrowerResults>> GetMostActiveBorrowerAsync(
        DateTimeOffset from, DateTimeOffset to,
        int limit,
        CancellationToken cancellationToken = default);

    Task<ReadingPaceResult> GetReadingPaceAsync(int loanId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReadersAlsoBorrowedResult>> GetReadersAlsoBorrowedAsync(
        int bookId,
        int limit,
        CancellationToken cancellationToken = default);
}
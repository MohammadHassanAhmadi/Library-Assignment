namespace Library.Service.Reports;

public sealed record BorrowedBookResults(
    int BookId,
    string Title,
    int BorrowCount);

public sealed record ActiveBorrowerResults(
    int BorrowerId,
    string Name,
    int BorrowerCount);

public sealed record ReadingPaceResult(
    int LoanId,
    int BorrowerId,
    int BookId,
    string BookTitle,
    string BorrowerName,
    int PageCount,
    double ElapsedDays,
    double PagesPerDay);

public sealed record ReadersAlsoBorrowedResult(
    int BookId,
    string Title,
    int BorrowedCount);
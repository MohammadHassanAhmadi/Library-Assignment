namespace Library.Api.Contract;

public sealed record ReadersAlsoBorrowedDto(int BookId, string Title, int BorrowerCount);

public sealed record ReadingPaceDto(
    int LoanId,
    int BorrowerId,
    string BorrowerName,
    int BookId,
    string BookTitle,
    int PageCount,
    double ElapsedDays,
    double PagesPerDay);

public sealed record ActiveBorrowerDto(int BorrowerId, string Name, int BorrowCount);

public sealed record BorrowedBookDto(int BookId, string Title, int BorrowCount);
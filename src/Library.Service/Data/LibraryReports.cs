using Library.Service.Reports;
using Microsoft.EntityFrameworkCore;

namespace Library.Service.Data;

public sealed class LibraryReports(LibraryDbContext dbContext) : ILibraryReport
{
    public async Task<IReadOnlyList<BorrowedBookResults>> GetMostBorrowedBooksAsync(int limit,
        CancellationToken cancellationToken = default)
    {
        ValidateLimit(limit);

        return await dbContext.Loans.AsNoTracking()
            .GroupBy(loan => new {loan.BookId, loan.Book.Title})
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key.BookId)
            .Take(limit)
            .Select(group => new BorrowedBookResults(group.Key.BookId, group.Key.Title, group.Count()))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ActiveBorrowerResults>> GetMostActiveBorrowerAsync(DateTimeOffset from,
        DateTimeOffset to,
        int limit,
        CancellationToken cancellationToken = default)
    {
        ValidateLimit(limit);

        if (from >= to)
            throw new ArgumentException(
                "'from' must be earlier than 'to'.", nameof(from));

        return await dbContext.Loans.AsNoTracking()
            .Where(loan => loan.BorrowedAt >= from && loan.BorrowedAt < to)
            .GroupBy(loan => new {loan.Borrower.Id, loan.Borrower.Name})
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key.Id)
            .Take(limit)
            .Select(group => new ActiveBorrowerResults(group.Key.Id, group.Key.Name, group.Count()))
            .ToListAsync(cancellationToken);
    }

    public async Task<ReadingPaceResult> GetReadingPaceAsync(int loanId, CancellationToken cancellationToken = default)
    {
        var loan = await dbContext.Loans
            .Select(x => new
            {
                x.Id,
                x.BookId,
                x.BorrowerId,
                BorrowerName = x.Borrower.Name,
                BookTitle = x.Book.Title,
                x.Book.PageCount,
                x.BorrowedAt,
                x.ReturnedAt
            })
            .FirstOrDefaultAsync(x => x.Id == loanId, cancellationToken);

        if (loan is null)
            throw new LoanNotFoundException(loanId);

        if (loan.ReturnedAt is null)
            throw new LoanNotReturnedException(loanId);


        var elapsedDays = (loan.ReturnedAt.Value - loan.BorrowedAt).TotalDays;
        var pagePerDay = (float) loan.PageCount / Math.Max(elapsedDays, 1d);

        return new ReadingPaceResult(loanId, loan.BorrowerId,
            loan.BookId,
            loan.BookTitle,
            loan.BorrowerName,
            loan.PageCount,
            elapsedDays,
            pagePerDay);
    }


    // What other books were borrowed by individuals who borrowed a specific book?
    public async Task<IReadOnlyList<ReadersAlsoBorrowedResult>> GetReadersAlsoBorrowedAsync(int bookId, int limit,
        CancellationToken cancellationToken = default)
    {
        ValidateLimit(limit);

        if (bookId < 1)
            throw new ArgumentException("Book id must be greater than zero.", nameof(bookId));

        var readerIds = dbContext.Loans
            .Where(loan => loan.BookId == bookId)
            .Select(loan => loan.BorrowerId);

        return await dbContext.Loans
            .AsNoTracking()
            .Where(loan => loan.BookId != bookId && readerIds.Contains(loan.BorrowerId))
            .GroupBy(loan => new {loan.BookId, loan.Book.Title})
            .OrderByDescending(group => group.Select(loan => loan.BorrowerId).Distinct().Count())
            .ThenBy(group => group.Key.BookId)
            .Take(limit)
            .Select(group => new ReadersAlsoBorrowedResult(
                group.Key.BookId,
                group.Key.Title,
                group.Select(loan => loan.BorrowerId).Distinct().Count()))
            .ToListAsync(cancellationToken);
    }


    private static void ValidateLimit(int limit)
    {
        if (limit is < 1 or > 100)
            throw new ArgumentOutOfRangeException(
                nameof(limit),
                limit,
                "Limit must be between 1 and 100.");
    }
}
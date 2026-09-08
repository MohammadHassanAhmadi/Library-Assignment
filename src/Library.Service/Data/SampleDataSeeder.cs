using Library.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Service.Data;

public static class SampleDataSeeder
{
    public static async Task SeedAsync(LibraryDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var hasExistingData = await dbContext.Books.AnyAsync(cancellationToken) ||
                              await dbContext.Borrowers.AnyAsync(cancellationToken);
        
        if (hasExistingData)
            return;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var lotrBook = new Book { Title = "Lord of the Rings - Fellowship of the Rings", PageCount = 500 };
        var hpBook = new Book { Title = "Harry Potter and the Chamber of Secrets", PageCount = 352 };
        var sqlBook = new Book { Title = "SQL Basics", PageCount = 464 };
        var apiBook = new Book { Title = "API Design", PageCount = 352 };

        dbContext.Books.AddRange(
            lotrBook,
            hpBook,
            sqlBook,
            apiBook
        );

        var alice = new Borrower { Name = "Alice" };
        var bob = new Borrower { Name = "Bob" };
        var carol = new Borrower { Name = "Carol" };

        dbContext.Borrowers.AddRange(alice, bob, carol);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.Loans.AddRange(
            CreateLoan(alice, lotrBook, UtcDate(1, 1), UtcDate(1, 11)),
            CreateLoan(alice, sqlBook, UtcDate(1, 12), UtcDate(1, 17)),
            CreateLoan(bob, lotrBook, UtcDate(1, 12), UtcDate(1, 22)),
            CreateLoan(bob, apiBook, UtcDate(1, 23), UtcDate(2, 2)),
            CreateLoan(carol, lotrBook, UtcDate(2, 1), UtcDate(2, 11)),
            CreateLoan(carol, sqlBook, UtcDate(2, 12), null)
        );

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static Loan CreateLoan(
        Borrower borrower,
        Book book,
        DateTimeOffset borrowedAt,
        DateTimeOffset? returnedAt)
    {
        return new Loan
        {
            BorrowerId = borrower.Id,
            BookId = book.Id,
            BorrowedAt = borrowedAt,
            ReturnedAt = returnedAt
        };
    }

    private static DateTimeOffset UtcDate(int month, int day) =>
        new(2026, month, day, 0, 0, 0, TimeSpan.Zero);

}
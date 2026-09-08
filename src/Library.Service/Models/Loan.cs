namespace Library.Service.Models;

public class Loan
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public int BorrowerId { get; set; }

    public DateTimeOffset BorrowedAt { get; set; }

    public DateTimeOffset? ReturnedAt { get; set; }

    public Book Book { get; set; } = null!;

    public Borrower Borrower { get; set; } = null!;
}
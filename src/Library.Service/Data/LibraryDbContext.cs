using Library.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Service.Data;

public sealed class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Borrower> Borrowers => Set<Borrower>();
    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(300);
            
            entity.ToTable("Books", table => 
                table.HasCheckConstraint("CK_Books_PageCount", "[PageCount] > 0"));
        });

        modelBuilder.Entity<Borrower>(entity =>
        {
            entity.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(150);
        });


        modelBuilder.Entity<Loan>(entity =>
        {
            entity.ToTable("Loans",
                table => table.HasCheckConstraint("CK_Loans_ReturnedAt",
                    "[ReturnedAt] IS NULL OR [ReturnedAt] >= [BorrowedAt]"));

            entity.HasOne(loan => loan.Book)
                .WithMany()
                .HasForeignKey(loan => loan.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(loan => loan.Borrower)
                .WithMany()
                .HasForeignKey(loan => loan.BorrowerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
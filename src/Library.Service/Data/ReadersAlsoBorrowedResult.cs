namespace Library.Service.Data;

public sealed record ReadersAlsoBorrowedResult(
    int BookId,
    string Title,
    int ReaderCount);
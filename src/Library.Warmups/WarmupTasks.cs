using System.Text;

namespace Library.Warmups;

public static class WarmupTasks
{
    public static bool IsPowerOfTwo(int bookId)
    {
        if (bookId <= 0)
            return false;

        return (bookId & (bookId - 1)) == 0;
    }


    public static string? ReverseTitle(string? bookTitle)
    {
        if(string.IsNullOrEmpty(bookTitle)) 
            return bookTitle;

        var bookTitleCharacters = bookTitle.ToCharArray();

        Array.Reverse(bookTitleCharacters);
        return new string(bookTitleCharacters);
    }



    public static string? ReplicateString(string? title, int count)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        var result = new StringBuilder(title.Length * count);
        for (var i = 0; i < count; i++)
        {
            result.Append(title);
        }

        return result.ToString();
    }

    public static void PrintOddBookIds(TextWriter? outputWriter = null)
    {
        outputWriter ??= Console.Out;

        for (var bookId = 1; bookId < 100; bookId+=2)
        {
            outputWriter.WriteLine(bookId);
        }
    }
}
using Library.Warmups;

namespace Library.UnitTests;

public class WarmupTasksTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void IsPowerOfTwo_NonPositiveInput_ReturnsFalse(int bookId)
    {
        Assert.False(WarmupTasks.IsPowerOfTwo(bookId));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(128)]
    [InlineData(1_073_741_824)]
    public void IsPowerOfTwo_PowerOfTwo_ReturnsTrue(int bookId)
    {
        Assert.True(WarmupTasks.IsPowerOfTwo(bookId));
    }

    [Theory]
    [InlineData(3)]
    [InlineData(9)]
    [InlineData(18)]
    [InlineData(126)]
    [InlineData(int.MaxValue)]
    public void IsPowerOfTwo_PositiveNonPowerOfTwo_ReturnsFalse(int bookId)
    {
        Assert.False(WarmupTasks.IsPowerOfTwo(bookId));
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ReverseTitle_WhenInputIsNullOrWhiteSpaceOrEmpty_ReturnsAsExpected(string bookTitle)
    {
        Assert.Equal(bookTitle, WarmupTasks.ReverseTitle(bookTitle));
    }


    [Theory]
    [InlineData("Hello", "olleH")]
    [InlineData("Moby Dick", "kciD yboM")]
    [InlineData(" Ali- O", "O -ilA ")]
    [InlineData("A", "A")]

    public void ReverseTitle_WhenInputIsValid_ReturnsAsExpected(string? bookTitle, string expected)
    {
        Assert.Equal(expected, WarmupTasks.ReverseTitle(bookTitle));
    }



    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(-128)]
    public void ReplicateTitle_WhenCountIsNegative_ThrowsArgumentOutOfRangeException(int count)
    {
        const string bookTitle = "Harry Potter";
        Assert.Throws<ArgumentOutOfRangeException>(() => WarmupTasks.ReplicateString(bookTitle, count));
    }


    [Fact]
    public void ReplicateTitle_WhenCountBookTitleIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => WarmupTasks.ReplicateString(null, 11));
    }

    [Theory]
    [InlineData("Read", 3 , "ReadReadRead")]
    [InlineData("Harry Potter", 0 , "")]
    [InlineData("A",6, "AAAAAA")]
    public void ReplicateTitle_WhenEveryThingIsOK_ResultMustBeAsExpected(string bookTitle, int count, string expected)
    {
        var actual = WarmupTasks.ReplicateString(bookTitle, count);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestPrintOddBookIds_PrintsOddNumbers()
    {
        using var stringWriter = new StringWriter();
        WarmupTasks.PrintOddBookIds(stringWriter);
        var actual = stringWriter.ToString();

        var oddNumbersInRange = Enumerable.Range(1, 99).Where(n => n % 2 != 0);
        var expected = string.Join(stringWriter.NewLine, oddNumbersInRange) + stringWriter.NewLine;
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void PrintOddBookIds_WithoutWriter_PrintsToConsole()
    {
        // it tests the console somehow
        using var stringWriter = new StringWriter();
        var originalWriter = Console.Out;

        try
        {
            Console.SetOut(stringWriter);

            WarmupTasks.PrintOddBookIds();

            var oddNumbersInRange = Enumerable.Range(1, 99).Where(n => n % 2 != 0);
            var expected = string.Join(stringWriter.NewLine, oddNumbersInRange) + stringWriter.NewLine;

            Assert.Equal(expected, stringWriter.ToString());
        }
        finally
        {
            Console.SetOut(originalWriter); //needs to restore the console
        }
    }
}
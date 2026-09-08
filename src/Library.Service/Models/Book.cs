namespace Library.Service.Models
{
    public class Book
    {
        public int Id { get; set; }

        public required string Title { get; set; }

        public int PageCount { get; set; }
    }
}

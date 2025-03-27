namespace BiblioBackend.DataContext.Entities
{
    public class Book
    {
        public int Id { get; set; }
        public string? Title { get; set; } // Nullable string
        public int AuthorId { get; set; }
        public Author? Author { get; set; } // Nullable referencia
        public int CategoryId { get; set; }
        public Category? Category { get; set; } // Nullable referencia
        public string? Description { get; set; } // Nullable string
        public bool IsAvailable { get; set; }
        public string? NumberInLibrary { get; set; } // Nullable string
        public BookQuality BookQuality { get; set; }
    }
}
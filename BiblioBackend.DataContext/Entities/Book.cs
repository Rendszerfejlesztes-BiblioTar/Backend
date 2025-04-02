namespace BiblioBackend.DataContext.Entities
{
    public class Book
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int AuthorId { get; set; }
        public virtual Author? Author { get; set; }
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }
        public string? Description { get; set; }
        public bool IsAvailable { get; set; }
        public string? NumberInLibrary { get; set; }
        public int BookQuality { get; set; }
    }
}
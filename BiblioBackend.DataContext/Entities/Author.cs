namespace BiblioBackend.DataContext.Entities
{
    public class Author
    {
        public int Id { get; set; }
        public string? Name { get; set; } // Nullable string
        public List<Book> Books { get; set; } = new List<Book>(); // Inicializálás üres listával
    }
}
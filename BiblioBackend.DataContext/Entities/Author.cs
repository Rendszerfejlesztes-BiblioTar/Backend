using System.Text.Json.Serialization;

namespace BiblioBackend.DataContext.Entities
{
    public class Author
    {
        public int Id { get; set; }
        public string? Name { get; set; } // Nullable string
        [JsonIgnore] //This is needed to prevent circular reference in swagger
        public List<Book> Books { get; set; } = new List<Book>(); // Inicializálás üres listával
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiblioBackend.DataContext.Entities;

namespace BiblioBackend.DataContext.Dtos.Book.Post
{
    public class BookPostDto
    {
        public string Title { get; set; } = "";
        public int AuthorId { get; set; }
        public int CategoryId { get; set; }
        public string? Description { get; set; }
        public bool IsAvailable { get; set; }
        public string? NumberInLibrary { get; set; }
        public BookQuality BookQuality { get; set; }

    }
}

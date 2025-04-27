using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiblioBackend.DataContext.Entities;

namespace BiblioBackend.DataContext.Dtos.Book.Modify
{
    public class BookQualityPatchDTO
    {
        public int Id { get; set; }
        public BookQuality BookQuality { get; set; }
        public string Email { get; set; }
    }
}

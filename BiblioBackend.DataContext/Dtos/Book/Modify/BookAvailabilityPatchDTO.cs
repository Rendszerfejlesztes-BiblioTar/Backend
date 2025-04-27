using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiblioBackend.DataContext.Dtos.Book.Modify
{
    public class BookAvailabilityPatchDTO
    {
        public int Id { get; set; }
        public bool IsAvailable { get; set; }
    }
}

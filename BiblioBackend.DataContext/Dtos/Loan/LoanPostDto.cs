using BiblioBackend.DataContext.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiblioBackend.DataContext.Dtos.Loan
{
    public class LoanPostDto

    {
        public string UserEmail { get; set; }
        public int BookId { get; set; }
        public DateTime StartTime { get; set; }
    }
}

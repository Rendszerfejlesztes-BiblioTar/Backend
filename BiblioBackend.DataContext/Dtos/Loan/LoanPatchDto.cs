﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiblioBackend.DataContext.Dtos.Loan
{
    public class LoanPatchDto
    {
        public int? Extensions { get; set; }
        public int? BookId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        
    }
}
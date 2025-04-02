﻿using BiblioBackend.DataContext.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiblioBackend.DataContext.Dtos.Reservation
{
    public class ReservationDetailDto
    {
        public int Id { get; set; }
        public Book? Book { get; set; }
        public UserDto? User { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime ReservationDate { get; set; } = DateTime.UtcNow;
        public DateTime ExpectedStart { get; set; }
        public DateTime ExpectedEnd { get; set; }
    }
}

using ApiApplication.Database.Entities;
using System;
using System.Collections.Generic;

namespace ApiApplication.Features.Model.ResponseModels
{
    public class Reservation
    {
        public Guid ReservationId { get; set; }
        public int SeatCount { get; set; }
        public List<Seat> SeatNumbers { get; set; }
        public AuditoriumEntity Auditorium { get; set; }
        public int ShowtimeId { get; set; }
        public ShowtimeEntity Showtime { get; set; }
        public MovieEntity Movie { get; set; }
        public DateTime SessionDate { get; set; }
    }
}

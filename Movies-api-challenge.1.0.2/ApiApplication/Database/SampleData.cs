using ApiApplication.Database.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiApplication.Database
{
    public class SampleData
    {
        public static void Initialize(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var context = serviceScope.ServiceProvider.GetService<CinemaContext>();
            context.Database.EnsureCreated();
            

            context.Auditoriums.Add(new AuditoriumEntity
            {
                Id = 1,
                Showtimes = new List<ShowtimeEntity> 
                { 
                    new ShowtimeEntity
                    {
                        Id = 1,
                        SessionDate = new DateTime(2023, 1, 1),
                        Movie = new MovieEntity
                        {
                            Id = 1,
                            Title = "Inception",
                            ImdbId = "tt1375666",
                            ReleaseDate = new DateTime(2010, 01, 14),
                            Stars = "Leonardo DiCaprio, Joseph Gordon-Levitt, Ellen Page, Ken Watanabe"                            
                        },
                        AuditoriumId = 1,
                    } 
                },
                Seats = GenerateSeats(1, 28, 22)
            });

            //DetacheSeats(context, 1, 28, 22);

            //context.Tickets.Add(new TicketEntity() {
            //    Id = new Guid(),
            //    ShowtimeId = 1,
            //    Paid = false,
            //    Seats = new List<SeatEntity> {
            //        new SeatEntity(){ 
            //            AuditoriumId= 1,
            //            Row = 1,
            //            SeatNumber = 2
            //        },
            //        new SeatEntity(){
            //            AuditoriumId= 1,
            //            Row = 1,
            //            SeatNumber = 3
            //        },
            //        new SeatEntity(){
            //            AuditoriumId= 1,
            //            Row = 1,
            //            SeatNumber = 4
            //        }
            //    },
            //    CreatedTime = DateTime.Now
            //});

            context.Auditoriums.Add(new AuditoriumEntity
            {
                Id = 2,
                Seats = GenerateSeats(2, 21, 18)
            });

            context.Auditoriums.Add(new AuditoriumEntity
            {
                Id = 3,
                Seats = GenerateSeats(3, 15, 21)
            });

            context.Auditoriums.Add(new AuditoriumEntity
            {
                Id = 4,
                Seats = GenerateSeats(4, 3, 3)
            });

            context.SaveChanges();
        }

        private static List<SeatEntity> GenerateSeats(int auditoriumId, short rows, short seatsPerRow)
        {
            var seats = new List<SeatEntity>();
            for (short r = 1; r <= rows; r++)
                for (short s = 1; s <= seatsPerRow; s++)
                    seats.Add(new SeatEntity { AuditoriumId = auditoriumId, Row = r, SeatNumber = s });

            return seats;
        }

        private static void DetacheSeats(CinemaContext _context, int auditoriumId, short rows, short seatsPerRow)
        {
            for (short r = 1; r <= rows; r++)
            {
                for (short sn = 1; sn <= seatsPerRow; sn++)
                {
                    var existingSeat = _context.Seats.Local.FirstOrDefault(s => s.AuditoriumId == auditoriumId && s.Row == r && s.SeatNumber == sn);

                    _context.Entry(existingSeat).State = EntityState.Detached;
                }
            }

        }
    }
}

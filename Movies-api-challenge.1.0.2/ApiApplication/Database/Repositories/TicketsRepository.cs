using ApiApplication.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Features.Model.ResponseModels;

namespace ApiApplication.Database.Repositories
{
    public class TicketsRepository : ITicketsRepository
    {
        private readonly CinemaContext _context;

        public TicketsRepository(CinemaContext context)
        {
            _context = context;
        }

        public Task<TicketEntity> GetAsync(Guid id, CancellationToken cancel)
        {
            return _context.Tickets.Include(x => x.Showtime).FirstOrDefaultAsync(x => x.Id == id, cancel);
        }

        public async Task<IEnumerable<TicketEntity>> GetEnrichedAsync(int showtimeId, CancellationToken cancel)
        {
            return await _context.Tickets
                .Include(x => x.Showtime)
                .Include(x => x.Seats)
                .Where(x => x.ShowtimeId == showtimeId)
                .ToListAsync(cancel);
        }

        public async Task<TicketEntity> CreateAsync(ShowtimeEntity showtime, IEnumerable<SeatEntity> selectedSeats, CancellationToken cancel)
        {
            // Create tickets for the reservation
            foreach (var seatNumber in selectedSeats)
            {
                var ticket = new TicketEntity
                {
                    ShowtimeId = showtime.Id,
                    Showtime = showtime,
                    Seats = selectedSeats.ToList()
                };

                // Fetch all tickets for the given showtime
                var tickets = await GetEnrichedAsync(showtime.Id, cancel);

                // Check if any of the fetched tickets match the selected seats
                var existingTicket = tickets.FirstOrDefault(t => t.Seats.Any(st => selectedSeats.Any(rst => rst.Row == st.Row && rst.SeatNumber == st.SeatNumber)));

                if (existingTicket == null)
                {
                    var newTicket = _context.Tickets.Add(ticket);
                    await _context.SaveChangesAsync(cancel);
                    return newTicket.Entity;
                }
            }

            return null;
        }

        public async Task<TicketEntity> ConfirmPaymentAsync(TicketEntity ticket, CancellationToken cancel)
        {
            ticket.Paid = true;
            _context.Update(ticket);
            await _context.SaveChangesAsync(cancel);
            return ticket;
        }

        public async Task<bool> AreSeatsSold(IEnumerable<SeatEntity> seats)
        {
            // Query tickets with Paid = true
            var paidTicketsQuery = await _context.Tickets.Include(s => s.Seats).Include(s => s.Showtime)
                .Where(ticket => ticket.Paid).ToListAsync();

            // Filter tickets based on requested seats
            foreach (var seat in seats)
            {
                paidTicketsQuery = paidTicketsQuery
                    .Where(ticket => ticket.Seats.Any(s => s.Row == seat.Row && s.SeatNumber == seat.SeatNumber && s.AuditoriumId == seat.AuditoriumId)).ToList();
            }

            // Execute the query and return the results
            return paidTicketsQuery.Count > 0;
        }

    }
}

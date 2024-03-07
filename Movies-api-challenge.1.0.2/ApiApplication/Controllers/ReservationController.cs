using ApiApplication.Database;
using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Features.Model.RequestModels;
using ApiApplication.Features.Model.ResponseModels;
using ApiApplication.Features.Seats.Command;
using ApiApplication.Features.Shows.Command.CreateShow;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProtoDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using static ProtoDefinitions.MoviesApi;

namespace ApiApplication.Controllers
{
    [Route("v1/Reservation")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public class ReservationController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly ITicketsRepository ticketReserveRepository;
        private readonly IShowtimesRepository showTimeRepository;
        private readonly ILogger<ShowTimesController> logger;
        private readonly CinemaContext _dbContext;

        public ReservationController(CinemaContext dbContext, ITicketsRepository ticketReserveRepository, IShowtimesRepository showTimeRepository, IMediator mediator, ILogger<ShowTimesController> logger)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.ticketReserveRepository = ticketReserveRepository ?? throw new ArgumentNullException(nameof(ticketReserveRepository));
            this.showTimeRepository = showTimeRepository ?? throw new ArgumentNullException(nameof(showTimeRepository));
            _dbContext = dbContext;
        }

        [HttpPost("reserve-seats")]
        public async Task<IActionResult> ReserveSeatsAsync([FromBody] ReserveSeatsRequest request) 
        {
            // Validate the request parameters
            if (!ValidateReservationRequest(request))
            {
                logger.LogError("Invalid reservation request.");
                throw new Exception("Invalid reservation request.");
            }

            // Check if the requested seats are available
            var seatAvailabilityResult = await CheckSeatAvailabilityAsync(request);

            if (!seatAvailabilityResult.Success)
            {
                return BadRequest(new { Message = seatAvailabilityResult.ErrorMessage });
            }

            // Check if seats are contiguous
            if (!AreSeatsContiguous(request.Seats))
            {
                return BadRequest(new { Message = "Requested seats must be contiguous." });
            }

            // Check if the seats have already been reserved within the last 10 minutes
            if (await AreSeatsAlreadyReserved(request.Seats))
            {
                return BadRequest(new { Message = "Seats have already been reserved." });
            }

            ReserveSeatsCommand commandRequest = new ReserveSeatsCommand(request.ShowTimeId, request.Seats);
            var result = await mediator.Send(commandRequest);
            logger.LogInformation("Seats reserved");

            return Ok(result);
        }

        [HttpPost("confirm-reservation")]
        public async Task<IActionResult> ConfirmReservation([FromBody] ConfirmSeatsRequest confirmSeatsRequest)
        {
            // Retrieve reservation from the database using reservationId
            var reservation = await _dbContext.Tickets.Include(s => s.Seats).Include(s => s.Showtime).FirstOrDefaultAsync(x => x.Id == confirmSeatsRequest.ReservationId);
            
            // Check if reservation exists
            if (reservation == null)
            {
                return NotFound("Reservation not found.");
            }
            //foreach (var item in reservation.Seats)
            //{
            //    item.AuditoriumId = reservation.Showtime.AuditoriumId;
            //}

            // Check if reservation is expired
            if (DateTime.UtcNow - reservation.CreatedTime > TimeSpan.FromMinutes(10) && reservation.Paid == false)
            {
                return BadRequest("Reservation has expired and cannot be confirmed.");
            }

            // Check if seats associated with reservation are already sold
            if (await ticketReserveRepository.AreSeatsSold(reservation.Seats))
            {
                return Conflict("Seats have already been sold.");
            }

            // Mark reservation as confirmed
            ConfirmReservationCommand commandRequest = new ConfirmReservationCommand(confirmSeatsRequest.ReservationId);
            var result = await mediator.Send(commandRequest);
            logger.LogInformation("Reservation confirmed successfully");

            return Ok(new { Message = "Reservation confirmed successfully.", Data = result });
        }

        #region Helper
        private bool ValidateReservationRequest(ReserveSeatsRequest request)
        {
            // Check if ShowTimeId is valid
            if (request.ShowTimeId == 0)
            {
                return false; // ShowTimeId is required and cannot be empty
            }

            // Check if SeatNumbers are provided and not empty
            if (request.Seats == null || request.Seats.Count == 0)
            {
                return false; // At least one seat number must be provided
            }

            return true;
        }

        private async Task<ReservationResult> CheckSeatAvailabilityAsync(ReserveSeatsRequest request)
        {
            try
            {
                var showtimes = await _dbContext.Showtimes.Where(a => a.Id == request.ShowTimeId).ToListAsync();
                var AuditoriumData = await _dbContext.Auditoriums.Where(a => a.Id == showtimes.FirstOrDefault().AuditoriumId).Include(a => a.Seats).FirstOrDefaultAsync();

                // Check if all objects in the main list have corresponding objects in the second list
                var hasError = request.Seats.Any(audiSeat => !AuditoriumData.Seats.Any(selctedSeat =>
                    audiSeat.Row == selctedSeat.Row && audiSeat.SeatNumber == selctedSeat.SeatNumber));

                if (hasError)
                {
                    logger.LogError("Error: One or more requested seats are already reserved.");
                    return new ReservationResult
                    {
                        Success = false,
                        ErrorMessage = "One or more requested seats are already reserved."
                    };
                }
                else
                {
                    logger.LogInformation("Seats in the auditorium have corresponding avilability as selected seats.");
                }

                // Query the database to check if the requested seats are available
                var reservedSeats = await _dbContext.Tickets.Include(s => s.Showtime).Where(t => t.Showtime.Id == request.ShowTimeId &&
                                t.Showtime.SessionDate > DateTime.Now // Only consider future showtimes
                                )
                    .ToListAsync();

                // Check if any of the requested seats are already reserved
                if (reservedSeats.Any())
                {
                    return new ReservationResult
                    {
                        Success = false,
                        ErrorMessage = "One or more requested seats are already reserved."
                    };
                }

                // No reserved seats found, seats are available
                return new ReservationResult { Success = true };
            }
            catch (Exception ex)
            {
                // Log the exception
                logger.LogError(ex, "Error occurred while checking seat availability");
                return new ReservationResult { Success = false, ErrorMessage = "An error occurred while checking seat availability." };
            }
        }

        private bool AreSeatsContiguous(List<SelectedSeats> seatNumbers)
        {
            // Sort the seat numbers in ascending order
            var sortedSeatNumbers = seatNumbers.OrderBy(s => s.Row).OrderBy(s => s.SeatNumber).ToList();

            // Check if the seat numbers form a continuous sequence without gaps
            for (int i = 0; i < sortedSeatNumbers.Count - 1; i++)
            {
                if (sortedSeatNumbers[i].SeatNumber + 1 != sortedSeatNumbers[i + 1].SeatNumber)
                {
                    return false; // Seats are not contiguous
                }
            }

            return true; // Seats are contiguous
        }

        private async Task<bool> AreSeatsAlreadyReserved(List<SelectedSeats> seatNumbers)
        {
            // Calculate the time threshold for considering reservations as expired (e.g., 10 minutes ago)
            var thresholdTime = DateTime.Now.AddMinutes(-10);

            // Query the database to find any reservations for the provided seat numbers within the last 10 minutes
            var allReservations = await _dbContext.Tickets.Include(s => s.Showtime).Include(s => s.Seats)
                                    .Where(t => t.CreatedTime >= thresholdTime)
                                    //.Where(s => seatNumbers.Any(x => s.Seats.Any(y => y.Row == x.Row && y.SeatNumber == x.SeatNumber)))
                                    .ToListAsync();


            //NOTE: in above query where clause with check seat number not working somehow, so checking with for loops.
            var IsReserved = false;

            if (allReservations.Count > 0)
            {
                foreach (var ticket in allReservations)
                {
                    foreach (var seat in ticket.Seats) {
                        foreach (var selectedSeat in seatNumbers) {
                            if (selectedSeat.Row == seat.Row && selectedSeat.SeatNumber == seat.SeatNumber) { 
                                IsReserved = true; break;
                            }
                        }
                        if (IsReserved)
                        {
                            break;
                        }
                    }
                    if (IsReserved)
                    {
                        break; 
                    }
                }
            }

            // Return true if reservations exist within the last 10 minutes, false otherwise
            return IsReserved;
        }

        #endregion
    }
}

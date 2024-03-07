using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Features.Model.ResponseModels;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Microsoft.Extensions.Caching.Distributed;
using ProtoDefinitions;
using Newtonsoft.Json;

namespace ApiApplication.Features.Seats.Command
{
    public class ConfirmReservationHandler : IRequestHandler<ConfirmReservationCommand, Reservation>
    {
        private readonly ITicketsRepository ticketReserveRepository;
        private readonly IShowtimesRepository showTimeRepository;
        private readonly IAuditoriumsRepository auditoriumsRepository;
        private readonly ILogger<ConfirmReservationHandler> logger;
        private readonly IMapper mapper;
        private readonly IDistributedCache _cache;

        public ConfirmReservationHandler(ITicketsRepository ticketReserveRepository, IShowtimesRepository showTimeRepository, IAuditoriumsRepository auditoriumsRepository, ILogger<ConfirmReservationHandler> logger, IMapper mapper, IDistributedCache cache)
        {
            this.ticketReserveRepository = ticketReserveRepository ?? throw new ArgumentNullException(nameof(ticketReserveRepository));
            this.showTimeRepository = showTimeRepository ?? throw new ArgumentNullException(nameof(showTimeRepository));
            this.auditoriumsRepository = auditoriumsRepository ?? throw new ArgumentNullException(nameof(auditoriumsRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.mapper = mapper;
            _cache = cache;
        }

        public async Task<Reservation> Handle(ConfirmReservationCommand request, CancellationToken cancellationToken)
        {
            // Retrieve reservation from the database using reservationId
            var reservation = await GetCashedReservation(request.ReservationId, cancellationToken);
            var response = await ticketReserveRepository.ConfirmPaymentAsync(reservation, cancellationToken);

            var showTimeInfo = await GetCashedShowTime(reservation.ShowtimeId, cancellationToken);
            var auditorium = await GetCashedAuditorium(showTimeInfo.AuditoriumId, cancellationToken);
            logger.LogInformation($"Confirm reservetion complete");
            var result = mapper.Map<Reservation>(response);
            if (result != null)
            {
                result.Auditorium = new AuditoriumEntity()
                {
                    Id = auditorium.Id,
                    Showtimes = auditorium.Showtimes
                };
                foreach (var item in result.SeatNumbers)
                {
                    item.AuditoriumId = showTimeInfo.AuditoriumId;
                    item.IsSold = true;
                }
            }
            return result;
        }

        #region Cache Helper
        private async Task<TicketEntity> GetCashedReservation(Guid request, CancellationToken cancellationToken) 
        {
            var cacheKey = $"MoviesApi:Ticket_{request}";

            //// Try to get the response from the cache
            //var cachedResponse = await _cache.GetStringAsync(cacheKey);
            //if (cachedResponse != null)
            //{
            //    logger.LogInformation($"Read Reservation data from cacheKey:{cacheKey}");
            //    return JsonConvert.DeserializeObject<TicketEntity>(cachedResponse);
            //}

            var reservation = await ticketReserveRepository.GetAsync(request, cancellationToken);

            // Cache the response for future use
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache for 10 minutes
            };
            //await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(reservation), cacheOptions);

            return reservation;
        }

        private async Task<ShowtimeEntity> GetCashedShowTime(int ShowTimeId, CancellationToken cancellationToken)
        {
            var cacheKey = $"MoviesApi:ShowTimeId_{ShowTimeId}";

            //// Try to get the response from the cache
            //var cachedResponse = await _cache.GetStringAsync(cacheKey);
            //if (cachedResponse != null)
            //{
            //    logger.LogInformation($"Read ShowTime data from cacheKey:{cacheKey}");
            //    return JsonConvert.DeserializeObject<ShowtimeEntity>(cachedResponse);
            //}

            var showTimeInfo = await showTimeRepository.GetAllAsync(s => s.Id == ShowTimeId, cancellationToken);

            // Cache the response for future use
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache for 10 minutes
            };
            //await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(showTimeInfo), cacheOptions);

            return showTimeInfo.FirstOrDefault();
        }

        private async Task<AuditoriumEntity> GetCashedAuditorium(int AuditoriumId, CancellationToken cancellationToken)
        {
            var cacheKey = $"MoviesApi:AuditoriumId_{AuditoriumId}";

            //// Try to get the response from the cache
            //var cachedResponse = await _cache.GetStringAsync(cacheKey);
            //if (cachedResponse != null)
            //{
            //    logger.LogInformation($"Read Auditorium data from cacheKey:{cacheKey}");
            //    return JsonConvert.DeserializeObject<AuditoriumEntity>(cachedResponse);
            //}

            var auditorium = await auditoriumsRepository.GetAsync(AuditoriumId, cancellationToken);

            // Cache the response for future use
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache for 10 minutes
            };
            //await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(auditorium), cacheOptions);

            return auditorium;
        }
        #endregion
    }

    public class ConfirmReservationCommand : IRequest<Reservation>
    {
        public ConfirmReservationCommand(Guid reservationId)
        {
            ReservationId = reservationId;
        }

        public Guid ReservationId { get; set; }
    }
}

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
using Newtonsoft.Json;

namespace ApiApplication.Features.Seats.Command
{
    public class ReserveSeatsHandler : IRequestHandler<ReserveSeatsCommand, Reservation>
    {
        private readonly ITicketsRepository ticketReserveRepository;
        private readonly IShowtimesRepository showTimeRepository;
        private readonly IAuditoriumsRepository auditoriumsRepository;
        private readonly ILogger<ReserveSeatsHandler> logger;
        private readonly IMapper mapper;
        private readonly IDistributedCache _cache;

        public ReserveSeatsHandler(ITicketsRepository ticketReserveRepository, IShowtimesRepository showTimeRepository, IAuditoriumsRepository auditoriumsRepository, ILogger<ReserveSeatsHandler> logger, IMapper mapper, IDistributedCache cache)
        {
            this.ticketReserveRepository = ticketReserveRepository ?? throw new ArgumentNullException(nameof(ticketReserveRepository));
            this.showTimeRepository = showTimeRepository ?? throw new ArgumentNullException(nameof(showTimeRepository));
            this.auditoriumsRepository = auditoriumsRepository ?? throw new ArgumentNullException(nameof(auditoriumsRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.mapper = mapper;
            _cache = cache;
        }

        public async Task<Reservation> Handle(ReserveSeatsCommand request, CancellationToken cancellationToken)
        {
            
            var showTimeInfo = await GetCashedShowTime(request.ShowTimeId, cancellationToken);
            var auditorium = await GetCashedAuditorium(showTimeInfo.AuditoriumId, cancellationToken);
            var selectedSeats = mapper.Map<IEnumerable<SeatEntity>>(request.Seats);
            var response = await ticketReserveRepository.CreateAsync(showTimeInfo, selectedSeats, cancellationToken);
            logger.LogInformation($"Reserve Seats complete");
            var result = mapper.Map<Reservation>(response);
            if (result != null) {
                result.Auditorium = new AuditoriumEntity() {
                    Id = auditorium.Id,
                    Showtimes = auditorium.Showtimes
                };
                foreach (var item in result.SeatNumbers) {
                    item.AuditoriumId = auditorium.Id; 
                }
            }
            return result;
        }

        #region Cache Helper
        private async Task<ShowtimeEntity> GetCashedShowTime(int ShowTimeId, CancellationToken cancellationToken)
        {
            var cacheKey = $"MoviesApi:ShowTimeId_{ShowTimeId}";

            //try
            //{
            //    // Try to get the response from the cache
            //    var cachedResponse = await _cache.GetStringAsync(cacheKey);
            //    if (cachedResponse != null)
            //    {
            //        logger.LogInformation($"Read ShowTime data from cacheKey:{cacheKey}");
            //        return JsonConvert.DeserializeObject<ShowtimeEntity>(cachedResponse);
            //    }
            //}
            //catch (Exception ex) 
            //{
            //    logger.LogError($"Error:Read ShowTime data from cacheKey:{cacheKey}, Reason: {ex.Message}");
            //}

            var showTimeInfo = await showTimeRepository.GetAllAsync(s => s.Id == ShowTimeId, cancellationToken);

            // Cache the response for future use
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache for 10 minutes
            };
            // Configure JsonSerializerSettings to ignore reference loops
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            //await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(showTimeInfo, jsonSettings), cacheOptions);

            return showTimeInfo.FirstOrDefault();
        }

        private async Task<AuditoriumEntity> GetCashedAuditorium(int AuditoriumId, CancellationToken cancellationToken)
        {
            //var cacheKey = $"MoviesApi:AuditoriumId_{AuditoriumId}";

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

    public class ReserveSeatsCommand : IRequest<Reservation>
    {
        public ReserveSeatsCommand(int id, ICollection<SelectedSeats> seats)
        {
            ShowTimeId = id;
            Seats = seats;
        }

        public int ShowTimeId { get; set; }
        public ICollection<SelectedSeats> Seats { get; set; }
    }
}

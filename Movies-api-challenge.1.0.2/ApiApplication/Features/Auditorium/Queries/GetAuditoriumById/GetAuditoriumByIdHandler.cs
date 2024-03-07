using ApiApplication.Database.Entities;
using AutoMapper;
using MediatR;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Features.Seats.Command;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using Google.Protobuf;
using ProtoDefinitions;
using Newtonsoft.Json;

namespace ApiApplication.Features.Auditorium.Queries.GetAuditoriumById
{
    public class GetAuditoriumByIdHandler : IRequestHandler<GetAuditoriumByIdQuery, AuditoriumEntity>
    {
        private readonly IAuditoriumsRepository auditoriumRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAuditoriumByIdHandler> _logger;
        private readonly IDistributedCache _cache;

        public GetAuditoriumByIdHandler(IAuditoriumsRepository auditoriumRepository, IMapper mapper, ILogger<GetAuditoriumByIdHandler> logger, IDistributedCache cache)
        {
            this.auditoriumRepository = auditoriumRepository ?? throw new ArgumentNullException(nameof(auditoriumRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger;
            _cache = cache;
        }

        public async Task<AuditoriumEntity> Handle(GetAuditoriumByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"MoviesApi:Auditorium_{request.searchId}";

            //// Try to get the response from the cache
            //var cachedResponse = await _cache.GetStringAsync(cacheKey);
            //if (cachedResponse != null)
            //{
            //    return JsonConvert.DeserializeObject<AuditoriumEntity>(cachedResponse);
            //}

            var result = await auditoriumRepository.GetAsync(request.searchId, cancellationToken);
            var mappedResult = _mapper.Map<AuditoriumEntity>(result);

            // Cache the response for future use
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache for 10 minutes
            };
            //await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(mappedResult), cacheOptions);

            _logger.LogInformation("Received Auditorium Data");
            return mappedResult;
        }
    }
}

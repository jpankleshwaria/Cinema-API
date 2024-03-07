using System;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Net.Client;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using ProtoDefinitions;

namespace ApiApplication
{
    public class ApiClientGrpc
    {
        private readonly MoviesApi.MoviesApiClient _client;
        private readonly IDistributedCache _cache;

        public ApiClientGrpc(IDistributedCache cache)
        {
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var channel =
                GrpcChannel.ForAddress("https://localhost:7443", new GrpcChannelOptions()
                {
                    HttpHandler = httpHandler
                });

            _client = new MoviesApi.MoviesApiClient(channel);
            _cache = cache;
        }

        public async Task<showListResponse> GetAllShowAsync()
        {
            var all = await _client.GetAllAsync(new Empty());
            all.Data.TryUnpack<showListResponse>(out var data);
            return data;
        }

        public async Task<showResponse> GetShowByIdAsync(string id)
        {
            var request = new IdRequest { Id = id };
            var response = await _client.GetByIdAsync(request);
            return response.Data.Unpack<showResponse>();
        }

        public async Task<showListResponse> SearchMoviesAsync(string searchText)
        {
            //TODO: resolve redis cache timeout
            //var cacheKey = $"MoviesApi:search_Movie_{searchText}";

            //// Try to get the response from the cache
            //var chk = _cache.GetString(cacheKey);
            //var cachedResponse = await _cache.GetStringAsync(cacheKey);
            //if (cachedResponse != null)
            //{
            //    return JsonParser.Default.Parse<showListResponse>(cachedResponse);
            //}

            // Fetch show details from ProvidedApi
            var request = new SearchRequest { Text = searchText };
            var response = await _client.SearchAsync(request);
            var showDetails = response.Data.Unpack<showListResponse>();

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

            //await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(showDetails, jsonSettings), cacheOptions);

            var responseT = _client.Search(request);
            return response.Data.Unpack<showListResponse>();
        }

        public async Task<showResponse> GetShowByIdRedisAsync(string id)
        {
            var cacheKey = $"MoviesApi:Show_{id}";

            //// Try to get the response from the cache
            //var cachedResponse = await _cache.GetStringAsync(cacheKey);
            //if (cachedResponse != null)
            //{
            //    return JsonParser.Default.Parse<showResponse>(cachedResponse);
            //}

            // Fetch show details from ProvidedApi
            var response = await _client.GetByIdAsync(new IdRequest { Id = id });
            var showDetails = response.Data.Unpack<showResponse>();

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

            //await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(showDetails, jsonSettings), cacheOptions);

            return showDetails;
        }
    }
}
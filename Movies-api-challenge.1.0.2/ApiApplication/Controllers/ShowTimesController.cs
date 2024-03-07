using ApiApplication.Database;
using ApiApplication.Database.Entities;
using ApiApplication.Features.Model.RequestModels;
using ApiApplication.Features.Shows.Command.CreateShow;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ProtoDefinitions.MoviesApi;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ApiApplication.Controllers
{
    [Route("v1/showTimes")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public class ShowTimesController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly ILogger<ShowTimesController> logger;
        private readonly ApiClientGrpc _moviesApiClient;
        private readonly CinemaContext _dbContext;

        public ShowTimesController(CinemaContext dbContext, IMediator mediator, ILogger<ShowTimesController> logger, ApiClientGrpc moviesApiClient)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _moviesApiClient = moviesApiClient;
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateShowtime([FromBody] CreateShowRequest request)
        {
            try
            {
                //TODO: resolve redis cache timeout 
                //// Fetch movie details from ProvidedApi
                //var movieDetails = await _moviesApiClient.SearchMoviesAsync(request.MovieTitle);

                //if (movieDetails != null)
                //{
                //    MovieEntity movie = new MovieEntity()
                //    {
                //        Id = Convert.ToInt32(movieDetails.Shows.FirstOrDefault().Id),
                //        Title = movieDetails.Shows.FirstOrDefault().FullTitle,
                //        ImdbId = movieDetails.Shows.FirstOrDefault().ImDbRating,
                //        ReleaseDate = new DateTime(Convert.ToInt32(movieDetails.Shows.FirstOrDefault().Year), 01, 14),
                //        Stars = movieDetails.Shows.FirstOrDefault().Crew
                //    };

                //    CreateShowCommand commandRequest = new CreateShowCommand(movie, request.SessionDate, request.AuditoriumId, 0);
                //    var showtime = await mediator.Send(commandRequest);
                //    return Ok(showtime);
                //}

                var movie = await _dbContext.Movies.Where(m => m.Title.ToLower().Contains(request.MovieTitle.ToLower())).FirstOrDefaultAsync();

                if (movie == null)
                {
                    movie = new MovieEntity() { Title = request.MovieTitle, ReleaseDate = new DateTime() };
                }

                CreateShowCommand commandRequest = new CreateShowCommand(movie, request.SessionDate, request.AuditoriumId, 0);
                var showtime = await mediator.Send(commandRequest);
                logger.LogInformation("ShowTime ceated");
                return Ok(showtime);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error creating showtime", Exception = ex.Message });
            }
        }
    }
}

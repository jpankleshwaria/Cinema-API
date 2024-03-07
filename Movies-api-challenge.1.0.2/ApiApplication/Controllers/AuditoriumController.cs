using ApiApplication.Database.Entities;
using ApiApplication.Features.Auditorium.Queries.GetAuditoriumById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ApiApplication.Controllers
{
    [Route("v1/auditorium")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public class AuditoriumController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly ILogger<AuditoriumController> logger;

        public AuditoriumController(IMediator mediator, ILogger<AuditoriumController> logger)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> AuditoriumDetailById(int id)
        {
            var command = new GetAuditoriumByIdQuery() { searchId = id };
            logger.LogInformation($"Controller : AuditoriumController, MethodName : AuditoriumDetailById Start");
            var result = await mediator.Send(command);
            if(result != null && result.Seats != null && result.Seats.Count > 10)
                result.Seats = result.Seats.Take(10).ToList(); // show top 10 seats, else it will be timeout

            logger.LogInformation("Controller : AuditoriumController, MethodName : AuditoriumDetailById End");
            return Ok(result);

        }
    }
}

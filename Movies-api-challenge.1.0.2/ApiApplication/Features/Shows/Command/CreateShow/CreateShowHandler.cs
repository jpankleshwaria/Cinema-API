using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using AutoMapper;
using System.Linq;
using FluentValidation;


namespace ApiApplication.Features.Shows.Command.CreateShow
{
    public class CreateShowHandler : IRequestHandler<CreateShowCommand, ShowtimeEntity>
    {
        private readonly IShowtimesRepository showtimesRepository;
        private readonly ILogger<CreateShowHandler> logger;
        private readonly IMapper mapper;
        private readonly CreateShowValidator validator;

        public CreateShowHandler(IShowtimesRepository showtimesRepository, ILogger<CreateShowHandler> logger, IMapper mapper)
        {
            this.showtimesRepository = showtimesRepository ?? throw new ArgumentNullException(nameof(showtimesRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.mapper = mapper;
            this.validator = new CreateShowValidator();
        }

        public async Task<ShowtimeEntity> Handle(CreateShowCommand request, CancellationToken cancellationToken)
        {
            // Validate the request
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                logger.LogError("Create show error with request body.");
                throw new ValidationException("Validation failed", validationResult.Errors);
            }

            // Request is valid, proceed with handling
            var showTimeInfo = mapper.Map<ShowtimeEntity>(request);
            var response = await showtimesRepository.CreateShowtime(showTimeInfo, cancellationToken);
            logger.LogInformation($"Show Created");
            return mapper.Map<ShowtimeEntity>(response);
        }
    }
}

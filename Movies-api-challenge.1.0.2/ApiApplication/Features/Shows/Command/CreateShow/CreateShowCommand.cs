using System.Reflection;
using System;
using ApiApplication.Database.Entities;
using System.Collections.Generic;
using MediatR;

namespace ApiApplication.Features.Shows.Command.CreateShow
{
    public class CreateShowCommand : IRequest<ShowtimeEntity>
    {
        public CreateShowCommand(MovieEntity movie, DateTime sessionDate, int auditoriumId, int id)
        {
            Movie = movie ?? throw new ArgumentNullException(nameof(movie));
            SessionDate = sessionDate;
            AuditoriumId = auditoriumId;
            Id = id;
        }

        public int Id { get; set; }
        public MovieEntity Movie { get; set; }
        public DateTime SessionDate { get; set; }
        public int AuditoriumId { get; set; }
    }

}

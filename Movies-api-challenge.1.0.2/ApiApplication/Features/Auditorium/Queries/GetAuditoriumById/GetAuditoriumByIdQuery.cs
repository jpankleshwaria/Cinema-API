using ApiApplication.Database.Entities;
using MediatR;

namespace ApiApplication.Features.Auditorium.Queries.GetAuditoriumById
{
    public class GetAuditoriumByIdQuery : IRequest<AuditoriumEntity>
    {
        public int searchId { get; set; }
    }
}

using FluentValidation;
using System.Net;

namespace ApiApplication.Features.Shows.Command.CreateShow
{
    public class CreateShowValidator : AbstractValidator<CreateShowCommand>
    {
        public CreateShowValidator()
        {
            RuleFor(x => x.Movie).NotEmpty().WithErrorCode(HttpStatusCode.BadRequest.ToString()).WithMessage("Movie is required");
            RuleFor(x => x.SessionDate).NotEmpty().WithErrorCode(HttpStatusCode.BadRequest.ToString()).WithMessage("Date is required");
            RuleFor(x => x.AuditoriumId).NotEmpty().WithErrorCode(HttpStatusCode.BadRequest.ToString()).WithMessage("Auditorium is required");
        }
    }

}

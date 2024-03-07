using MediatR;

namespace ApiApplication.Utility.Interfaces
{
    /// <summary>
    /// this request implementes for which command's have validation
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    public interface IValidationRequest<TResponse> : IRequest<TResponse>
    {

    }
}

using FluentResults;
using MediatR;
using Streetcode.Auth.BLL.DTO.Auth;

namespace Streetcode.Auth.BLL.MediatR.LoginWithGoogle
{
    public record LoginWithGoogleCommand(LoginWithGoogleDTO LoginGoogle) : IRequest<Result<(TokenResponseDTO Response, string RefreshToken)>>;
}

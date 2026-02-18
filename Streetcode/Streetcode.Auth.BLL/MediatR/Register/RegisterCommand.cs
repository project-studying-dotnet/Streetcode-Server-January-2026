using FluentResults;
using MediatR;
using Streetcode.Auth.BLL.DTO.Auth;

namespace Streetcode.Auth.BLL.MediatR.Register
{
    public record RegisterCommand(RegisterRequestDTO RegisterRequest)
        : IRequest<Result<(TokenResponseDTO Response, string RefreshToken)>>;
}

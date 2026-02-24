using FluentResults;
using MediatR;
using Streetcode.Auth.BLL.DTO.Auth;

namespace Streetcode.Auth.BLL.MediatR.Login
{
    public record LoginCommand(LoginRequestDTO LoginRequest)
        : IRequest<Result<(TokenResponseDTO Response, string RefreshToken)>>;
}
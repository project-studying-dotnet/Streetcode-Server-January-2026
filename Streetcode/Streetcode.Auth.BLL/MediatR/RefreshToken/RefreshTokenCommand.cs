using FluentResults;
using MediatR;
using Streetcode.Auth.BLL.DTO.Auth;

namespace Streetcode.Auth.BLL.MediatR.RefreshToken
{
    public record RefreshTokenCommand(string RefreshToken)
        : IRequest<Result<(TokenResponseDTO Response, string RefreshToken)>>;
}

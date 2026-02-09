using FluentResults;
using MediatR;
using Streetcode.Auth.BLL.DTO;

namespace Streetcode.Auth.BLL.MediatR.Login
{
    public record LoginCommand(LoginDTO loginRequestDto) : IRequest<Result<TokenResponseDTO>>;
}
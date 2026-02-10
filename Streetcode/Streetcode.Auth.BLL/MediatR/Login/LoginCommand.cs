using FluentResults;
using MediatR;
using Streetcode.Auth.BLL.DTO;
using Streetcode.Auth.BLL.DTO.Auth;

namespace Streetcode.Auth.BLL.MediatR.Login
{
    public record LoginCommand(LoginDTO loginRequestDto) : IRequest<Result<TokenResponseDTO>>;
}
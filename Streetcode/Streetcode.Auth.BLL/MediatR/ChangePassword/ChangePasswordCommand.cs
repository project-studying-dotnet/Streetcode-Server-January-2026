using FluentResults;
using MediatR;
using Streetcode.Auth.BLL.DTO.Auth;

namespace Streetcode.Auth.BLL.MediatR.ChangePassword
{
    public record ChangePasswordCommand(ChangePasswordRequestDTO Request) : IRequest<Result<Unit>>;
}
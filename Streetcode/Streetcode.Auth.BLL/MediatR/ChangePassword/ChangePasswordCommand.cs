using FluentResults;
using MediatR;
using Streetcode.Auth.BLL.DTO.Auth;

namespace Streetcode.Auth.BLL.MediatR.ChangePassword
{
    public record ChangePasswordCommand(ChangePasswordRequestDTO Request, string Email) : IRequest<Result<Unit>>;
}
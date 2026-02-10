using FluentResults;
using MediatR;

namespace Streetcode.Auth.BLL.MediatR.Logout
{
    public record LogoutCommand(string RefreshToken)
        : IRequest<Result<Unit>>;
}

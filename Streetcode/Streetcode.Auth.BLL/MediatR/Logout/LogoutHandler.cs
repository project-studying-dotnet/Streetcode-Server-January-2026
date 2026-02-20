using FluentResults;
using MediatR;
using Streetcode.Auth.BLL.Interfaces;

namespace Streetcode.Auth.BLL.MediatR.Logout
{
    public class LogoutHandler : IRequestHandler<LogoutCommand, Result<Unit>>
    {
        private readonly ITokenService _tokenService;

        public LogoutHandler(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task<Result<Unit>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);

            return Result.Ok(Unit.Value);
        }
    }
}

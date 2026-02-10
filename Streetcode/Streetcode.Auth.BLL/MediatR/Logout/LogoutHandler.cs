using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Streetcode.Auth.BLL.DTO.Auth;
using Streetcode.Auth.BLL.Interfaces;
using Streetcode.Auth.BLL.MediatR.Login;
using Streetcode.Auth.DAL.Entities;

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

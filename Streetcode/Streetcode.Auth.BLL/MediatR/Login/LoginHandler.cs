using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Streetcode.Auth.BLL.DTO.Auth;
using Streetcode.Auth.BLL.Interfaces;
using Streetcode.Auth.DAL.Entities;

namespace Streetcode.Auth.BLL.MediatR.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<TokenResponseDTO>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public LoginCommandHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<Result<TokenResponseDTO>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var someData = new TokenResponseDTO { };

            return Result.Ok(someData);
        }
    }
}
using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Streetcode.Auth.BLL.DTO.Auth;
using Streetcode.Auth.BLL.DTO.Users;
using Streetcode.Auth.BLL.Interfaces;
using Streetcode.Auth.DAL.Entities;

namespace Streetcode.Auth.BLL.MediatR.Login
{
    public class LoginHandler : IRequestHandler<LoginCommand, Result<(TokenResponseDTO, string)>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public LoginHandler(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IMapper mapper,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<Result<(TokenResponseDTO, string)>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.LoginRequest.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.LoginRequest.Password))
            {
                return Result.Fail("Invalid email or password");
            }

            var (accessToken, refreshToken) = await _tokenService.GenerateTokensAsync(user);

            var responseDto = new TokenResponseDTO
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"] !)),
                User = _mapper.Map<UserDTO>(user)
            };

            return Result.Ok((responseDto, refreshToken.Token));
        }
    }
}
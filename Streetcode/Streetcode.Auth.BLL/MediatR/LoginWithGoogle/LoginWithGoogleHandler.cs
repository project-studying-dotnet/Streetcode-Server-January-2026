using AutoMapper;
using FluentResults;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Streetcode.Auth.BLL.DTO.Auth;
using Streetcode.Auth.BLL.DTO.Users;
using Streetcode.Auth.BLL.Interfaces;
using Streetcode.Auth.BLL.MediatR.Login;
using Streetcode.Auth.DAL.Entities;
using Streetcode.Shared.DTO.Events;
using Streetcode.Shared.Enums;

namespace Streetcode.Auth.BLL.MediatR.LoginWithGoogle
{
    public class LoginWithGoogleHandler : IRequestHandler<LoginWithGoogleCommand, Result<(TokenResponseDTO, string)>>
    {
        private readonly ILogger<LoginWithGoogleHandler> _logger;
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IPublishEndpoint _publishEndpoint;

        public LoginWithGoogleHandler(ILogger<LoginWithGoogleHandler> logger, ITokenService tokenService, UserManager<ApplicationUser> userManager, IMapper mapper, IConfiguration configuration, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _tokenService = tokenService;
            _userManager = userManager;
            _mapper = mapper;
            _configuration = configuration;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Result<(TokenResponseDTO, string)>> Handle(LoginWithGoogleCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.LoginGoogle.Email);

            if (user == null)
            {
                user = _mapper.Map<ApplicationUser>(request.LoginGoogle);

                var createResult = await _userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                {
                    return Result.Fail(createResult.Errors.Select(e => e.Description));
                }

                await _userManager.AddToRoleAsync(user, nameof(UserRole.User));

                await _publishEndpoint.Publish(
                    new UserRegisteredEvent
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        Name = user.Name,
                        Surname = user.Surname,
                        Role = UserRole.User
                    },
                    cancellationToken);
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
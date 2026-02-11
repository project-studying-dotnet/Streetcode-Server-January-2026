using AutoMapper;
using FluentResults;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Streetcode.Auth.BLL.DTO.Auth;
using Streetcode.Auth.BLL.DTO.Users;
using Streetcode.Auth.BLL.Interfaces;
using Streetcode.Auth.DAL.Entities;
using Streetcode.Auth.DAL.Enums;
using Streetcode.Shared.DTO.Events;

namespace Streetcode.Auth.BLL.MediatR.Register
{
    public class RegisterHandler : IRequestHandler<RegisterCommand, Result<(TokenResponseDTO, string)>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly IPublishEndpoint _publishEndpoint;

        public RegisterHandler(
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ITokenService tokenService,
            IConfiguration configuration,
            IPublishEndpoint publishEndpoint)
        {
            _userManager = userManager;
            _mapper = mapper;
            _tokenService = tokenService;
            _configuration = configuration;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Result<(TokenResponseDTO, string)>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.RegisterRequest.Email);
            if (existingUser != null)
            {
                return Result.Fail($"User with email {request.RegisterRequest.Email} already exists.");
            }

            var user = _mapper.Map<ApplicationUser>(request.RegisterRequest);

            var createResult = await _userManager.CreateAsync(user, request.RegisterRequest.Password);
            if (!createResult.Succeeded)
            {
                return Result.Fail(createResult.Errors.Select(e => e.Description));
            }

            await _userManager.AddToRoleAsync(user, nameof(UserRole.User));

            var (accessToken, refreshToken) = await _tokenService.GenerateTokensAsync(user);

            await _publishEndpoint.Publish(
                new UserRegisteredEvent
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        Name = user.Name,
                        Surname = user.Surname,
                        PhoneNumber = user.PhoneNumber
                    }, cancellationToken);

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

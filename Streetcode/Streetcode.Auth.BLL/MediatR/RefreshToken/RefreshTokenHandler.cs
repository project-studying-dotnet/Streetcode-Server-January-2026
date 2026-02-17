using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Configuration;
using Streetcode.Auth.BLL.DTO.Auth;
using Streetcode.Auth.BLL.DTO.Users;
using Streetcode.Auth.BLL.Exceptions;
using Streetcode.Auth.BLL.Interfaces;

namespace Streetcode.Auth.BLL.MediatR.RefreshToken
{
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<(TokenResponseDTO, string)>>
    {
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public RefreshTokenHandler(
            ITokenService tokenService,
            IMapper mapper,
            IConfiguration configuration)
        {
            _tokenService = tokenService;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<Result<(TokenResponseDTO, string)>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var (newAccess, newRefresh) = await _tokenService.RotateRefreshTokenAsync(request.RefreshToken);

                var responseDto = new TokenResponseDTO
                {
                    AccessToken = newAccess,
                    AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"] !)),
                    User = _mapper.Map<UserDTO>(newRefresh.User)
                };

                return Result.Ok((responseDto, newRefresh.Token));
            }
            catch (UnauthorizedException ex)
            {
                return Result.Fail(ex.Message);
            }
        }
    }
}
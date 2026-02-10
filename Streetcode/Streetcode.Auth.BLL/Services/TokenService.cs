using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Streetcode.Auth.BLL.Interfaces;
using Streetcode.Auth.DAL.Entities;
using Streetcode.Auth.DAL.Repositories.Interfaces;

namespace Streetcode.Auth.BLL.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public TokenService(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _configuration = configuration;
            _userManager = userManager;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<(string AccessToken, RefreshToken RefreshToken)> GenerateTokensAsync(ApplicationUser user)
        {
            var accessToken = await GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken(user.Id);

            await _refreshTokenRepository.CreateAsync(refreshToken);

            return (accessToken, refreshToken);
        }

        public async Task<(string AccessToken, RefreshToken RefreshToken)> RotateRefreshTokenAsync(string oldToken)
        {
            var existingToken = await _refreshTokenRepository.GetByTokenAsync(oldToken);

            if (existingToken == null)
            {
                throw new Exception("Invalid token");
            }

            if (existingToken.Revoked)
            {
                throw new Exception("Token is already revoked");
            }

            if (existingToken.Expires < DateTime.UtcNow)
            {
                throw new Exception("Token expired");
            }

            existingToken.Revoked = true;
            await _refreshTokenRepository.UpdateAsync(existingToken);

            return await GenerateTokensAsync(existingToken.User);
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var existingToken = await _refreshTokenRepository.GetByTokenAsync(token);

            if (existingToken != null && !existingToken.Revoked)
            {
                existingToken.Revoked = true;
                await _refreshTokenRepository.UpdateAsync(existingToken);
            }
        }

        public async Task RevokeAllAsync(string userId)
        {
            await _refreshTokenRepository.RevokeAllAsync(userId);
        }

        private async Task<string> GenerateAccessToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] !));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"] !)),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private RefreshToken GenerateRefreshToken(string userId)
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            var tokenString = Convert.ToBase64String(randomNumber);

            return new RefreshToken
            {
                Token = tokenString,
                Expires = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenExpireDays"] !)),
                CreatedAt = DateTime.UtcNow,
                Revoked = false,
                UserId = userId
            };
        }
    }
}

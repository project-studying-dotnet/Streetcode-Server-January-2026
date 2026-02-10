using Streetcode.Auth.DAL.Entities;

namespace Streetcode.Auth.BLL.Interfaces
{
    public interface ITokenService
    {
        Task<(string AccessToken, RefreshToken RefreshToken)> GenerateTokensAsync(ApplicationUser user);

        Task<(string AccessToken, RefreshToken RefreshToken)> RotateRefreshTokenAsync(string oldRefreshToken);

        Task RevokeRefreshTokenAsync(string token);

        Task RevokeAllAsync(string userId);
    }
}

using Streetcode.Auth.DAL.Entities;

namespace Streetcode.Auth.DAL.Repositories.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task CreateAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task UpdateAsync(RefreshToken refreshToken);
        Task RevokeAllAsync(string userId);
        Task DeleteAsync(RefreshToken refreshToken);
        Task<int> DeleteExpiredAsync();
    }
}

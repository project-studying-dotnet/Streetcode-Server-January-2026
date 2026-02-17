using Microsoft.EntityFrameworkCore;
using Streetcode.Auth.DAL.Entities;
using Streetcode.Auth.DAL.Persistence;
using Streetcode.Auth.DAL.Repositories.Interfaces;

namespace Streetcode.Auth.DAL.Repositories.Realizations
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AuthDbContext _context;

        public RefreshTokenRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .SingleOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task UpdateAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task RevokeAllAsync(string userId)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(t => t.UserId == userId && !t.Revoked)
                .ToListAsync();

            if (activeTokens.Any())
            {
                foreach (var token in activeTokens)
                {
                    token.Revoked = true;
                }

                _context.RefreshTokens.UpdateRange(activeTokens);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Remove(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteExpiredAsync()
        {
            await _context.RefreshTokens
                .Where(t => t.Expires < DateTime.UtcNow)
                .ExecuteDeleteAsync();
            await _context.SaveChangesAsync();
        }
    }
}

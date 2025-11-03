using Microsoft.EntityFrameworkCore;
using SmileCareAPI.Data;
using SmileCareAPI.Models;
using SmileCareAPI.Repositories.Interface;

namespace SmileCareAPI.Repositories.Implementation
{

    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _context;

        public AuthRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // User Operations
        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<User> GetUserByEmailOrUsernameAsync(string emailOrUsername)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == emailOrUsername || u.UserName == emailOrUsername);
        }

        // Refresh Token Operations
        public async Task<RefreshToken> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<RefreshToken> GetRefreshTokenByIdAsync(int id)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Id == id);
        }

        public async Task<List<RefreshToken>> GetUserActiveRefreshTokensAsync(string userId)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId &&
                            !rt.IsRevoked &&
                            rt.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync();
        }

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
        }

        public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Update(refreshToken);
        }

        public async Task RevokeRefreshTokenAsync(RefreshToken refreshToken)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            _context.RefreshTokens.Update(refreshToken);
        }

        public async Task RevokeAllUserRefreshTokensAsync(string userId, string exceptToken = null)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                if (exceptToken == null || token.Token != exceptToken)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                }
            }
        }

        // OTP Operations
        public async Task<OtpCode> GetLatestOtpAsync(string userId)
        {
            return await _context.OtpCodes
                .Where(otp => otp.UserId == userId)
                .OrderByDescending(otp => otp.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<OtpCode> GetOtpByCodeAsync(string userId, string code)
        {
            return await _context.OtpCodes
                .FirstOrDefaultAsync(otp => otp.UserId == userId && otp.Code == code);
        }

        public async Task AddOtpAsync(OtpCode otpCode)
        {
            await _context.OtpCodes.AddAsync(otpCode);
        }

        public async Task UpdateOtpAsync(OtpCode otpCode)
        {
            _context.OtpCodes.Update(otpCode);
        }

        public async Task InvalidateOldOtpsAsync(string userId)
        {
            var oldOtps = await _context.OtpCodes
                .Where(otp => otp.UserId == userId && !otp.IsUsed)
                .ToListAsync();

            foreach (var otp in oldOtps)
            {
                otp.IsUsed = true;
                otp.UsedAt = DateTime.UtcNow;
            }
        }

        // Activity Log Operations
        public async Task AddActivityLogAsync(ActivityLog activityLog)
        {
            await _context.ActivityLogs.AddAsync(activityLog);
        }

        public async Task<List<ActivityLog>> GetUserActivityLogsAsync(string userId, int take = 50)
        {
            return await _context.ActivityLogs
                .Where(al => al.UserId == userId)
                .OrderByDescending(al => al.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}

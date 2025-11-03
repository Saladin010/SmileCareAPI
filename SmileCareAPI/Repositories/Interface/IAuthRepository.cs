using SmileCareAPI.Models;

namespace SmileCareAPI.Repositories.Interface
{
    public interface IAuthRepository
    {
        // User Operations
        Task<User> GetUserByIdAsync(string userId);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> GetUserByEmailOrUsernameAsync(string emailOrUsername);

        // Refresh Token Operations
        Task<RefreshToken> GetRefreshTokenAsync(string token);
        Task<RefreshToken> GetRefreshTokenByIdAsync(int id);
        Task<List<RefreshToken>> GetUserActiveRefreshTokensAsync(string userId);
        Task AddRefreshTokenAsync(RefreshToken refreshToken);
        Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
        Task RevokeRefreshTokenAsync(RefreshToken refreshToken);
        Task RevokeAllUserRefreshTokensAsync(string userId, string exceptToken = null);

        // OTP Operations
        Task<OtpCode> GetLatestOtpAsync(string userId);
        Task<OtpCode> GetOtpByCodeAsync(string userId, string code);
        Task AddOtpAsync(OtpCode otpCode);
        Task UpdateOtpAsync(OtpCode otpCode);
        Task InvalidateOldOtpsAsync(string userId);

        // Activity Log Operations
        Task AddActivityLogAsync(ActivityLog activityLog);
        Task<List<ActivityLog>> GetUserActivityLogsAsync(string userId, int take = 50);

        Task<bool> SaveChangesAsync();
    }
}

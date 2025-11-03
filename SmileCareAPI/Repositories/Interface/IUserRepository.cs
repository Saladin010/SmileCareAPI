using SmileCareAPI.DTOs;
using SmileCareAPI.Models;

namespace SmileCareAPI.Repositories.Interface
{
    public interface IUserRepository
    {
        Task<UsersListResponseDto> GetAllUsersAsync(
            UserRole? role = null,
            bool? isActive = null,
            string? search = null,
            int page = 1,
            int pageSize = 20
        );

        Task<UserDetailDto?> GetUserByIdAsync(string userId);

        Task<(bool Success, string? UserId, string? ErrorMessage)> CreateUserAsync(CreateUserDto dto);

        Task<(bool Success, string? ErrorMessage)> UpdateUserAsync(string userId, UpdateUserDto dto);

        Task<(bool Success, string? ErrorMessage)> DeleteUserAsync(string userId);

        Task<(bool Success, bool IsActive, string? ErrorMessage)> ToggleUserStatusAsync(string userId);

        Task<(bool Success, string? ProfilePictureUrl, string? ErrorMessage)> UploadProfilePictureAsync(string userId, IFormFile file);

        Task<(bool Success, string? ErrorMessage)> DeleteProfilePictureAsync(string userId);

        Task<UserStatisticsDto> GetUserStatisticsAsync();
    }
}

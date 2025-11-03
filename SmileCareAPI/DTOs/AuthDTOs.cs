using SmileCareAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.DTOs
{
    // Register DTOs
    public class RegisterDto
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(500)]
        public string? DetailedAddress { get; set; }
    }

    // Login DTOs
    public class LoginDto
    {
        [Required]
        public string EmailOrUsername { get; set; }

        [Required]
        public string Password { get; set; }
    }

    // Token DTOs
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }

    // Password DTOs
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class VerifyOtpDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string OtpCode { get; set; }
    }

    public class ResetPasswordDto
    {
        [Required]
        public string ResetToken { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
    }

    // Response DTOs
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public UserDto User { get; set; }
    }

    public class RegisterResponseDto
    {
        public string Message { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string? ProfilePicture { get; set; }
    }

    public class SessionDto
    {
        public int Id { get; set; }
        public string DeviceName { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsCurrentSession { get; set; }
    }

    public class MessageResponseDto
    {
        public string Message { get; set; }
    }

    public class OtpResponseDto
    {
        public string Message { get; set; }
        public string ResetToken { get; set; }
    }
}

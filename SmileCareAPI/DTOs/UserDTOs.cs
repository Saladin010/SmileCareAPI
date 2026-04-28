using SmileCareAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.DTOs
{

    public class CreateUserDto
    {
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [MaxLength(100)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public UserRole Role { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(500)]
        public string? DetailedAddress { get; set; }

        // Doctor specific fields
        [MaxLength(200)]
        public string? Specialization { get; set; }

        [MaxLength(100)]
        public string? LicenseNumber { get; set; }

        public int? YearsOfExperience { get; set; }

        [MaxLength(500)]
        public string? Credentials { get; set; }

        // Patient specific fields
        [MaxLength(100)]
        public string? EmergencyContactName { get; set; }

        [MaxLength(50)]
        public string? EmergencyContactRelationship { get; set; }

        [MaxLength(20)]
        public string? EmergencyContactPhone { get; set; }

        [MaxLength(20)]
        public string? EmergencyContactAlternativePhone { get; set; }

        public string? AssignedDoctorId { get; set; }

        [MaxLength(20)]
        public string? BloodType { get; set; }
    }

    public class UpdateUserDto
    {
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(500)]
        public string? DetailedAddress { get; set; }

        // Doctor specific fields
        [MaxLength(200)]
        public string? Specialization { get; set; }

        [MaxLength(100)]
        public string? LicenseNumber { get; set; }

        public int? YearsOfExperience { get; set; }

        [MaxLength(500)]
        public string? Credentials { get; set; }

        // Patient specific fields
        [MaxLength(100)]
        public string? EmergencyContactName { get; set; }

        [MaxLength(50)]
        public string? EmergencyContactRelationship { get; set; }

        [MaxLength(20)]
        public string? EmergencyContactPhone { get; set; }

        [MaxLength(20)]
        public string? EmergencyContactAlternativePhone { get; set; }

        public string? AssignedDoctorId { get; set; }

        [MaxLength(20)]
        public string? BloodType { get; set; }
    }

    public class UserListItemDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public UserRole Role { get; set; }
        public string RoleName { get; set; }
        public Gender Gender { get; set; }
        public string GenderName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ProfilePicture { get; set; }
    }

    public class UserDetailDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public UserRole Role { get; set; }
        public string RoleName { get; set; }
        public Gender Gender { get; set; }
        public string GenderName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? City { get; set; }
        public string? DetailedAddress { get; set; }
        public string? ProfilePicture { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DoctorInfoDto? Doctor { get; set; }
        public PatientInfoDto? Patient { get; set; }
    }

    public class DoctorInfoDto
    {
        public string Specialization { get; set; }
        public string? LicenseNumber { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? Credentials { get; set; }
        public decimal? AverageRating { get; set; }
        public int TotalPatients { get; set; }
        public int CompletedAppointments { get; set; }
    }

    public class PatientInfoDto
    {
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactRelationship { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactAlternativePhone { get; set; }
        public string? AssignedDoctorId { get; set; }
        public string? AssignedDoctorName { get; set; }
        public string? BloodType { get; set; }
        public int TotalVisits { get; set; }
        public DateTime? LastVisitDate { get; set; }
    }

    public class UsersListResponseDto
    {
        public List<UserListItemDto> Users { get; set; }
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
    }

    public class UserStatisticsDto
    {
        public int TotalUsers { get; set; }

        public int TotalDoctors { get; set; }
        public int TotalReceptionists { get; set; }
        public int TotalPatients { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
    }

    public class ToggleStatusResponseDto
    {
        public string Message { get; set; }
        public bool IsActive { get; set; }
    }

    public class UploadProfilePictureResponseDto
    {
        public string Message { get; set; }
        public string ProfilePictureUrl { get; set; }
    }
}

using SmileCareAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.DTOs
{
    public class DoctorListDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Specialization { get; set; }
        public string? LicenseNumber { get; set; }
        public int? YearsOfExperience { get; set; }
        public decimal? AverageRating { get; set; }
        public int TotalPatients { get; set; }
        public int CompletedAppointments { get; set; }
        public string? ProfilePicture { get; set; }
        public bool IsActive { get; set; }
    }

    public class DoctorDetailsDto
    {
        public UserInfoDto User { get; set; }
        public DoctorInfoDto Doctor { get; set; }
        public List<WorkingHoursDto> WorkingHours { get; set; }
        public DoctorStatisticsDto Statistics { get; set; }
    }

    public class UserInfoDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? ProfilePicture { get; set; }
    }



    public class DoctorStatisticsDto
    {
        public int AppointmentsToday { get; set; }
        public int AppointmentsThisWeek { get; set; }
        public int AppointmentsThisMonth { get; set; }
    }

    public class WorkingHoursDto
    {
        public int Id { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public bool IsOpen { get; set; }
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public TimeSpan? BreakStartTime { get; set; }
        public TimeSpan? BreakEndTime { get; set; }
    }

    public class PatientListDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public int TotalVisits { get; set; }
        public DateTime? LastVisitDate { get; set; }
        public string? ProfilePicture { get; set; }
    }

    public class AppointmentListDto
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public AppointmentType Type { get; set; }
        public AppointmentStatus Status { get; set; }
        public PatientBasicInfoDto Patient { get; set; }
        public string? ReasonForVisit { get; set; }
    }

    public class PatientBasicInfoDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? ProfilePicture { get; set; }
    }

    public class AvailableSlotDto
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class AvailableSlotsResponseDto
    {
        public DateTime Date { get; set; }
        public List<AvailableSlotDto> AvailableSlots { get; set; }
    }

    public class DoctorHolidayDto
    {
        public int Id { get; set; }
        public DateTime HolidayDate { get; set; }
        public string HolidayName { get; set; }
    }

    // ======================= Request DTOs =======================

    public class UpdateWorkingHoursDto
    {
        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        public bool IsOpen { get; set; }

        public TimeSpan? OpenTime { get; set; }

        public TimeSpan? CloseTime { get; set; }

        public TimeSpan? BreakStartTime { get; set; }

        public TimeSpan? BreakEndTime { get; set; }
    }

    public class AddHolidayDto
    {
        [Required]
        public DateTime HolidayDate { get; set; }

        [Required]
        [MaxLength(200)]
        public string HolidayName { get; set; }
    }

    // ======================= Paginated Response DTOs =======================

    public class PaginatedDoctorsResponseDto
    {
        public List<DoctorListDto> Doctors { get; set; }
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class PaginatedPatientsResponseDto
    {
        public List<PatientListDto> Patients { get; set; }
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class PaginatedAppointmentsResponseDto
    {
        public List<AppointmentListDto> Appointments { get; set; }
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}

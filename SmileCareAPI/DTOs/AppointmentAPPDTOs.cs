using SmileCareAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.DTOs
{
    public class CreateAppointmentAPPDto
    {
        [Required]
        public string PatientId { get; set; }

        [Required]
        public string DoctorId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        public int DurationMinutes { get; set; } = 30;

        [Required]
        public AppointmentType Type { get; set; }

        [MaxLength(500)]
        public string? ReasonForVisit { get; set; }

        public bool SendSmsConfirmation { get; set; } = false;

        public bool SendEmailConfirmation { get; set; } = false;
    }

    public class UpdateAppointmentAPPDto
    {
        public DateTime? AppointmentDate { get; set; }

        public TimeSpan? StartTime { get; set; }

        public int? DurationMinutes { get; set; }

        public AppointmentType? Type { get; set; }

        [MaxLength(500)]
        public string? ReasonForVisit { get; set; }
    }

    public class CancelAppointmentAPPDto
    {
        [Required]
        [MaxLength(1000)]
        public string CancellationReason { get; set; }

        public bool NotifyPatient { get; set; } = true;
    }

    public class RescheduleAppointmentAPPDto
    {
        [Required]
        public DateTime NewDate { get; set; }

        [Required]
        public TimeSpan NewStartTime { get; set; }

        public bool NotifyPatient { get; set; } = true;
    }

    public class ConfirmAppointmentAPPDto
    {
        public bool SendConfirmation { get; set; } = true;
    }

    public class SendReminderAPPDto
    {
        public bool Sms { get; set; } = false;

        public bool Email { get; set; } = false;
    }

    // Response DTOs
    public class AppointmentListItemAPPDto
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public AppointmentType Type { get; set; }
        public string TypeName { get; set; }
        public AppointmentStatus Status { get; set; }
        public string StatusName { get; set; }
        public PatientBasicAPPDto Patient { get; set; }
        public DoctorBasicAPPDto Doctor { get; set; }
        public string? ReasonForVisit { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AppointmentDetailAPPDto
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public AppointmentType Type { get; set; }
        public string TypeName { get; set; }
        public AppointmentStatus Status { get; set; }
        public string StatusName { get; set; }
        public string? ReasonForVisit { get; set; }
        public string? CancellationReason { get; set; }
        public bool ConfirmationSentSms { get; set; }
        public bool ConfirmationSentEmail { get; set; }
        public bool ReminderSent { get; set; }
        public PatientDetailAPPDto Patient { get; set; }
        public DoctorDetailAPPDto Doctor { get; set; }
        public MedicalRecordBasicAPPDto? MedicalRecord { get; set; }
        public CreatedByAPPDto? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class PatientBasicAPPDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfilePicture { get; set; }
    }

    public class PatientDetailAPPDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public string GenderName { get; set; }
        public string? ProfilePicture { get; set; }
    }

    public class DoctorBasicAPPDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Specialization { get; set; }
        public string? ProfilePicture { get; set; }
    }

    public class DoctorDetailAPPDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Specialization { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? ProfilePicture { get; set; }
    }

    public class MedicalRecordBasicAPPDto
    {
        public int Id { get; set; }
        public string? ChiefComplaint { get; set; }
        public string? ExaminationNotes { get; set; }
    }

    public class CreatedByAPPDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
    }

    public class AppointmentListResponseAPPDto
    {
        public List<AppointmentListItemAPPDto> Appointments { get; set; }
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class AvailableSlotAPPDto
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class AvailableSlotsResponseAPPDto
    {
        public DateTime Date { get; set; }
        public string DoctorId { get; set; }
        public List<AvailableSlotAPPDto> Slots { get; set; }
    }

    public class AppointmentStatisticsAPPDto
    {
        public int TotalAppointments { get; set; }
        public int Pending { get; set; }
        public int Confirmed { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
        public int NoShow { get; set; }
        public decimal CompletionRate { get; set; }
        public List<AppointmentsByDayAPPDto> AppointmentsByDay { get; set; }
        public List<AppointmentsByTypeAPPDto> AppointmentsByType { get; set; }
        public List<AppointmentsByDoctorAPPDto> AppointmentsByDoctor { get; set; }
    }

    public class AppointmentsByDayAPPDto
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class AppointmentsByTypeAPPDto
    {
        public AppointmentType Type { get; set; }
        public string TypeName { get; set; }
        public int Count { get; set; }
    }

    public class AppointmentsByDoctorAPPDto
    {
        public string DoctorName { get; set; }
        public int Count { get; set; }
    }

    // Query Parameters DTOs
    public class AppointmentQueryParametersAPPDto
    {
        public DateTime? Date { get; set; }
        public string? DoctorId { get; set; }
        public string? PatientId { get; set; }
        public AppointmentStatus? Status { get; set; }
        public AppointmentType? Type { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class StatisticsQueryParametersAPPDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? DoctorId { get; set; }
    }

    public class UpcomingAppointmentsQueryAPPDto
    {
        public int Days { get; set; } = 7;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class AvailableSlotsQueryAPPDto
    {
        [Required]
        public string DoctorId { get; set; }

        [Required]
        public DateTime Date { get; set; }
    }
}

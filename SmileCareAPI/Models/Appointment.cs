using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.Models
{
    public class Appointment
    {
        public int Id { get; set; }

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

        [Required]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        [MaxLength(500)]
        public string? ReasonForVisit { get; set; }

        [MaxLength(1000)]
        public string? CancellationReason { get; set; }

        public bool ConfirmationSentSms { get; set; } = false;

        public bool ConfirmationSentEmail { get; set; } = false;

        public bool ReminderSent { get; set; } = false;

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public Doctor Doctor { get; set; }

        public MedicalRecord? MedicalRecord { get; set; }
    }
}

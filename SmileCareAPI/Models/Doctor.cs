using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.Models
{
    
    public class Doctor
    {
        [Key]
        public string UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Specialization { get; set; }

        [MaxLength(100)]
        public string? LicenseNumber { get; set; }

        public int? YearsOfExperience { get; set; }

        [MaxLength(500)]
        public string? Credentials { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal? AverageRating { get; set; }

        public int TotalPatients { get; set; } = 0;

        public int CompletedAppointments { get; set; } = 0;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        public ICollection<Patient> Patients { get; set; } = new List<Patient>();

        public ICollection<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();

        public ICollection<Treatment> Treatments { get; set; } = new List<Treatment>();

        public ICollection<AIResult> AIResults { get; set; } = new List<AIResult>();

        public ICollection<WorkingHours> WorkingHours { get; set; } = new List<WorkingHours>();
    }
}

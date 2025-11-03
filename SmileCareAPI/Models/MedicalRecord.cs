using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.Models
{
   
    public class MedicalRecord
    {
        public int Id { get; set; }

        [Required]
        public string PatientId { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [Required]
        public DateTime VisitDate { get; set; }

        [MaxLength(1000)]
        public string? ChiefComplaint { get; set; }

        [MaxLength(2000)]
        public string? ExaminationNotes { get; set; }

        [MaxLength(200)]
        public string? BloodPressure { get; set; }

        [MaxLength(200)]
        public string? Temperature { get; set; }

        [MaxLength(1000)]
        public string? Vitals { get; set; }

        [MaxLength(2000)]
        public string? DoctorNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; }

        [ForeignKey(nameof(AppointmentId))]
        public Appointment Appointment { get; set; }

        public ICollection<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();

        public ICollection<Treatment> Treatments { get; set; } = new List<Treatment>();
    }
}

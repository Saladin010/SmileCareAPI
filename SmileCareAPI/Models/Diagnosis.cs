using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.Models
{
  
    public class Diagnosis
    {
        public int Id { get; set; }

        [Required]
        public string PatientId { get; set; }

        [Required]
        public string DoctorId { get; set; }

        public int? MedicalRecordId { get; set; }

        [Required]
        [MaxLength(200)]
        public string DiagnosisName { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public SeverityLevel Severity { get; set; }

        [Required]
        public DiagnosisStatus Status { get; set; } = DiagnosisStatus.Active;

        [MaxLength(100)]
        public string? AffectedArea { get; set; }

        public int? ToothNumber { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsAIAssisted { get; set; } = false;

        public int? AIResultId { get; set; }

        public DateTime DiagnosisDate { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedDate { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public Doctor Doctor { get; set; }

        [ForeignKey(nameof(MedicalRecordId))]
        public MedicalRecord? MedicalRecord { get; set; }

        [ForeignKey(nameof(AIResultId))]
        public AIResult? AIResult { get; set; }

        public ICollection<Treatment> Treatments { get; set; } = new List<Treatment>();

        public ICollection<PatientImage> Images { get; set; } = new List<PatientImage>();
    }
}

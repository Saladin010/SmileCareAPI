using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.Models
{
    
    public class AIResult
    {
        public int Id { get; set; }

        [Required]
        public string PatientId { get; set; }

        [Required]
        public string DoctorId { get; set; }

        [Required]
        public int ImageId { get; set; }

        [Required]
        public DiagnosisType PrimaryResult { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal ConfidenceScore { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal HealthyProbability { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal CavityProbability { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal GumDiseaseProbability { get; set; }

        [MaxLength(50)]
        public string? ModelVersion { get; set; }

        public double ProcessingTimeSeconds { get; set; }

        public string? HeatmapImageUrl { get; set; }

        public bool IsConfirmedByDoctor { get; set; } = false;

        public bool DoctorAgreement { get; set; } = false;

        [MaxLength(2000)]
        public string? DoctorNotes { get; set; }

        [MaxLength(200)]
        public string? DoctorFinalDiagnosis { get; set; }

        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

        public DateTime? DoctorReviewDate { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public Doctor Doctor { get; set; }

        [ForeignKey(nameof(ImageId))]
        public PatientImage Image { get; set; }

        public Diagnosis? Diagnosis { get; set; }
    }
}

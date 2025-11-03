using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.Models
{
 
    public class PatientImage
    {
        public int Id { get; set; }

        [Required]
        public string PatientId { get; set; }

        [Required]
        public string UploadedBy { get; set; }

        [Required]
        public ImageType ImageType { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        public int? ToothNumber { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(500)]
        public string? Tags { get; set; }

        [MaxLength(100)]
        public string? FileName { get; set; }

        public long FileSizeBytes { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public bool IsAnalyzedByAI { get; set; } = false;

        public int? VisitId { get; set; }

        public int? DiagnosisId { get; set; }

        public int? TreatmentId { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; }

        public ICollection<AIResult> AIResults { get; set; } = new List<AIResult>();
    }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.Models
{
    public class Document
    {
        public int Id { get; set; }

        [Required]
        public string PatientId { get; set; }

        [Required]
        public string UploadedBy { get; set; }

        [Required]
        public DocumentType DocumentType { get; set; }

        [Required]
        [MaxLength(200)]
        public string FileName { get; set; }

        [Required]
        public string FileUrl { get; set; }

        public long FileSizeBytes { get; set; }

        [MaxLength(100)]
        public string? FileExtension { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; }
    }
}

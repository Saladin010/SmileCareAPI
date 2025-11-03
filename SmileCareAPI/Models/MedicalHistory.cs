using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.Models
{
   
    public class MedicalHistory
    {
        [Key]
        public string PatientId { get; set; }

        [MaxLength(1000)]
        public string? ChronicDiseases { get; set; }

        [MaxLength(1000)]
        public string? Allergies { get; set; }

        [MaxLength(1000)]
        public string? CurrentMedications { get; set; }

        [MaxLength(1000)]
        public string? PreviousDentalSurgeries { get; set; }

        [MaxLength(2000)]
        public string? AdditionalNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; }
    }
}

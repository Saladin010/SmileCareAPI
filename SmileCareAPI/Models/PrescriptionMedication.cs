using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.Models
{
   
    public class PrescriptionMedication
    {
        public int Id { get; set; }

        [Required]
        public int PrescriptionId { get; set; }

        [Required]
        [MaxLength(200)]
        public string MedicationName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Dosage { get; set; }

        [Required]
        [MaxLength(100)]
        public string Frequency { get; set; }

        [Required]
        [MaxLength(100)]
        public string Duration { get; set; }

        [MaxLength(500)]
        public string? SpecialInstructions { get; set; }

        [ForeignKey(nameof(PrescriptionId))]
        public Prescription Prescription { get; set; }
    }
}
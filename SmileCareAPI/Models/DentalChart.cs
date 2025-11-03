using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.Models
{
    public class DentalChart
    {
        public int Id { get; set; }

        [Required]
        public string PatientId { get; set; }

        [Required]
        public int ToothNumber { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime? LastUpdated { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; }
    }
}

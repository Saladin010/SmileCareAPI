using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.Models
{
   
    public class TreatmentSession
    {
        public int Id { get; set; }

        [Required]
        public int TreatmentId { get; set; }

        public int SessionNumber { get; set; }

        public DateTime? ScheduledDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public bool IsCompleted { get; set; } = false;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? SessionCost { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(TreatmentId))]
        public Treatment Treatment { get; set; }
    }
}
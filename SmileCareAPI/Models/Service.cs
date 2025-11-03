using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string ServiceName { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int DefaultDurationMinutes { get; set; } = 30;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? DefaultPrice { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}

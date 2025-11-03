using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.Models
{
  
    public class ActivityLog
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Action { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(200)]
        public string? DeviceName { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        public bool IsSuccess { get; set; } = true;

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
    }
}
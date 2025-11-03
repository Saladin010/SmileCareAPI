using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.Models
{
  
    public class SystemSettings
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string SettingKey { get; set; }

        [Required]
        public string SettingValue { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

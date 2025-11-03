using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.Models
{
   
    public class WorkingHours
    {
        public int Id { get; set; }

        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        public bool IsOpen { get; set; } = true;

        public TimeSpan? OpenTime { get; set; }

        public TimeSpan? CloseTime { get; set; }

        public TimeSpan? BreakStartTime { get; set; }

        public TimeSpan? BreakEndTime { get; set; }

        public string? DoctorId { get; set; }

        public DateTime? HolidayDate { get; set; }

        [MaxLength(200)]
        public string? HolidayName { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

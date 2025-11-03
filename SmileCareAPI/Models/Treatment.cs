using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.Models
{
  
    public class Treatment
    {
        public int Id { get; set; }

        [Required]
        public string PatientId { get; set; }

        [Required]
        public string DoctorId { get; set; }

        public int? DiagnosisId { get; set; }

        public int? MedicalRecordId { get; set; }

        [Required]
        [MaxLength(200)]
        public string TreatmentName { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public TreatmentStatus Status { get; set; } = TreatmentStatus.Planned;

        public int? ToothNumber { get; set; }

        [MaxLength(100)]
        public string? AffectedArea { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? ExpectedCompletionDate { get; set; }

        public DateTime? ActualCompletionDate { get; set; }

        public int TotalSessions { get; set; } = 1;

        public int CompletedSessions { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? EstimatedCost { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PaidAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? BalanceAmount { get; set; }

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [MaxLength(2000)]
        public string? ProgressNotes { get; set; }

        [MaxLength(1000)]
        public string? Complications { get; set; }

        [MaxLength(1000)]
        public string? PatientFeedback { get; set; }

        public string? BeforeImageUrl { get; set; }

        public string? AfterImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public Doctor Doctor { get; set; }

        [ForeignKey(nameof(DiagnosisId))]
        public Diagnosis? Diagnosis { get; set; }

        [ForeignKey(nameof(MedicalRecordId))]
        public MedicalRecord? MedicalRecord { get; set; }

        public ICollection<TreatmentSession> Sessions { get; set; } = new List<TreatmentSession>();

        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

        public ICollection<PatientImage> Images { get; set; } = new List<PatientImage>();
    }
}

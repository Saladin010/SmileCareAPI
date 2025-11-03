using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.Models
{
   
    public class Prescription
    {
        public int Id { get; set; }

        [Required]
        public string PatientId { get; set; }

        [Required]
        public string DoctorId { get; set; }

        public int? TreatmentId { get; set; }

        public int? MedicalRecordId { get; set; }

        [Required]
        public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

        public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Active;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime? CompletedDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public Doctor Doctor { get; set; }

        [ForeignKey(nameof(TreatmentId))]
        public Treatment? Treatment { get; set; }

        [ForeignKey(nameof(MedicalRecordId))]
        public MedicalRecord? MedicalRecord { get; set; }

        public ICollection<PrescriptionMedication> Medications { get; set; } = new List<PrescriptionMedication>();
    }
}
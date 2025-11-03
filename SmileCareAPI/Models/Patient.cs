using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace SmileCareAPI.Models
{
  
    public class Patient
    {
        [Key]
        public string UserId { get; set; }

        [MaxLength(100)]
        public string? EmergencyContactName { get; set; }

        [MaxLength(50)]
        public string? EmergencyContactRelationship { get; set; }

        [MaxLength(20)]
        public string? EmergencyContactPhone { get; set; }

        [MaxLength(20)]
        public string? EmergencyContactAlternativePhone { get; set; }

        public string? AssignedDoctorId { get; set; }

        [MaxLength(20)]
        public string? BloodType { get; set; }

        public int TotalVisits { get; set; } = 0;

        public DateTime? LastVisitDate { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [ForeignKey(nameof(AssignedDoctorId))]
        public Doctor? AssignedDoctor { get; set; }

        public MedicalHistory? MedicalHistory { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

        public ICollection<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();

        public ICollection<Treatment> Treatments { get; set; } = new List<Treatment>();

        public ICollection<PatientImage> Images { get; set; } = new List<PatientImage>();

        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }

}

using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.DTOs
{
    // GET All Medical Records Response DTO
    public class MedGetAllRecordsResponseDto
    {
        public List<MedRecordSummaryDto> Records { get; set; } = new();
        public int TotalCount { get; set; }
    }

    // Medical Record Summary DTO (for list view)
    public class MedRecordSummaryDto
    {
        public int Id { get; set; }
        public DateTime VisitDate { get; set; }
        public MedPatientBasicDto Patient { get; set; } = new();
        public MedAppointmentBasicDto Appointment { get; set; } = new();
        public string? ChiefComplaint { get; set; }
        public string? ExaminationNotes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Patient Basic Info DTO
    public class MedPatientBasicDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    // Appointment Basic Info DTO
    public class MedAppointmentBasicDto
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    // GET Single Medical Record Response DTO
    public class MedGetRecordDetailsResponseDto
    {
        public int Id { get; set; }
        public DateTime VisitDate { get; set; }
        public string? ChiefComplaint { get; set; }
        public string? ExaminationNotes { get; set; }
        public string? BloodPressure { get; set; }
        public string? Temperature { get; set; }
        public string? Vitals { get; set; }
        public string? DoctorNotes { get; set; }
        public MedPatientDetailsDto Patient { get; set; } = new();
        public MedAppointmentBasicDto Appointment { get; set; } = new();
        public List<MedDiagnosisDto> Diagnoses { get; set; } = new();
        public List<MedTreatmentDto> Treatments { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // Patient Details DTO
    public class MedPatientDetailsDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
    }

    // Diagnosis DTO
    public class MedDiagnosisDto
    {
        public int Id { get; set; }
        public string DiagnosisName { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    // Treatment DTO
    public class MedTreatmentDto
    {
        public int Id { get; set; }
        public string TreatmentName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    // POST Create Medical Record Request DTO
    public class MedCreateRecordRequestDto
    {
        [Required]
        public string PatientId { get; set; } = string.Empty;

        [Required]
        public int AppointmentId { get; set; }

        [Required]
        public DateTime VisitDate { get; set; }

        [MaxLength(1000)]
        public string? ChiefComplaint { get; set; }

        [MaxLength(2000)]
        public string? ExaminationNotes { get; set; }

        [MaxLength(200)]
        public string? BloodPressure { get; set; }

        [MaxLength(200)]
        public string? Temperature { get; set; }

        [MaxLength(1000)]
        public string? Vitals { get; set; }

        [MaxLength(2000)]
        public string? DoctorNotes { get; set; }
    }

    // POST/PUT Response DTO
    public class MedRecordOperationResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int? RecordId { get; set; }
    }

    // PUT Update Medical Record Request DTO
    public class MedUpdateRecordRequestDto
    {
        [MaxLength(1000)]
        public string? ChiefComplaint { get; set; }

        [MaxLength(2000)]
        public string? ExaminationNotes { get; set; }

        [MaxLength(200)]
        public string? BloodPressure { get; set; }

        [MaxLength(200)]
        public string? Temperature { get; set; }

        [MaxLength(1000)]
        public string? Vitals { get; set; }

        [MaxLength(2000)]
        public string? DoctorNotes { get; set; }
    }

    // GET Patient Medical Records Response DTO
    public class MedGetPatientRecordsResponseDto
    {
        public List<MedRecordSummaryDto> Records { get; set; } = new();
        public int TotalCount { get; set; }
    }
}

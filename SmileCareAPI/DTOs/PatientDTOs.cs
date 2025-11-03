using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.DTOs
{

    // Filter DTO
    public class PatientFilterDto
    {
        public string? Search { get; set; }
        public string? Gender { get; set; }
        public string? Status { get; set; }
        public string? AssignedDoctorId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // Response DTOs
    public class PatientListItemDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string? City { get; set; }
        public string? ProfilePicture { get; set; }
        public int TotalVisits { get; set; }
        public DateTime? LastVisitDate { get; set; }
        public bool IsActive { get; set; }
        public AssignedDoctorDto? AssignedDoctor { get; set; }
    }

    public class PaginatedPatientsResponseDtoP
    {
        public List<PatientListItemDto> Patients { get; set; }
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class PatientDetailsResponseDto
    {
        public UserInfoDtoP User { get; set; }
        public PatientInfoDtoP Patient { get; set; }
        public MedicalHistoryDto? MedicalHistory { get; set; }
        public PatientStatisticsDetailsDto Statistics { get; set; }
    }

    public class UserInfoDtoP
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? City { get; set; }
        public string? DetailedAddress { get; set; }
        public string? ProfilePicture { get; set; }
    }

    public class PatientInfoDtoP
    {
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactRelationship { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactAlternativePhone { get; set; }
        public AssignedDoctorDetailedDto? AssignedDoctor { get; set; }
        public string? BloodType { get; set; }
        public int TotalVisits { get; set; }
        public DateTime? LastVisitDate { get; set; }
    }

    public class AssignedDoctorDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class AssignedDoctorDetailedDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Specialization { get; set; }
    }

    public class MedicalHistoryDto
    {
        public string? ChronicDiseases { get; set; }
        public string? Allergies { get; set; }
        public string? CurrentMedications { get; set; }
        public string? PreviousDentalSurgeries { get; set; }
        public string? AdditionalNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class PatientStatisticsDetailsDto
    {
        public int TotalAppointments { get; set; }
        public int UpcomingAppointments { get; set; }
        public int TotalDiagnoses { get; set; }
        public int TotalTreatments { get; set; }
        public int TotalImages { get; set; }
    }

    // Create/Update DTOs
    public class CreatePatientDto
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        public string? City { get; set; }
        public string? DetailedAddress { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? AssignedDoctorId { get; set; }
    }

    public class UpdatePatientDto
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        public string? City { get; set; }
        public string? DetailedAddress { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactRelationship { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactAlternativePhone { get; set; }
        public string? BloodType { get; set; }
    }

    public class UpdateMedicalHistoryDto
    {
        public string? ChronicDiseases { get; set; }
        public string? Allergies { get; set; }
        public string? CurrentMedications { get; set; }
        public string? PreviousDentalSurgeries { get; set; }
        public string? AdditionalNotes { get; set; }
    }

    public class AssignDoctorDto
    {
        [Required]
        public string DoctorId { get; set; }
    }

    // Appointment DTOs
    public class PatientAppointmentDto
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public AppointmentDoctorDto Doctor { get; set; }
        public string? ReasonForVisit { get; set; }
    }

    public class AppointmentDoctorDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Specialization { get; set; }
    }

    public class PaginatedAppointmentsResponseDtoP
    {
        public List<PatientAppointmentDto> Appointments { get; set; }
        public int TotalCount { get; set; }
    }

    // Medical Record DTOs
    public class PatientMedicalRecordDto
    {
        public int Id { get; set; }
        public DateTime VisitDate { get; set; }
        public string? ChiefComplaint { get; set; }
        public string? ExaminationNotes { get; set; }
        public RecordDoctorDto Doctor { get; set; }
        public List<RecordDiagnosisDto> Diagnoses { get; set; }
        public List<RecordTreatmentDto> Treatments { get; set; }
    }

    public class RecordDoctorDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class RecordDiagnosisDto
    {
        public int Id { get; set; }
        public string DiagnosisName { get; set; }
        public string Severity { get; set; }
    }

    public class RecordTreatmentDto
    {
        public int Id { get; set; }
        public string TreatmentName { get; set; }
        public string Status { get; set; }
    }

    public class PaginatedMedicalRecordsResponseDto
    {
        public List<PatientMedicalRecordDto> Records { get; set; }
        public int TotalCount { get; set; }
    }

    // Diagnosis DTOs
    public class PatientDiagnosisDto
    {
        public int Id { get; set; }
        public string DiagnosisName { get; set; }
        public string? Description { get; set; }
        public string Severity { get; set; }
        public string Status { get; set; }
        public DateTime DiagnosisDate { get; set; }
        public DiagnosisDoctorDto Doctor { get; set; }
        public bool IsAIAssisted { get; set; }
        public string? AffectedArea { get; set; }
        public int? ToothNumber { get; set; }
    }

    public class DiagnosisDoctorDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class PaginatedDiagnosesResponseDto
    {
        public List<PatientDiagnosisDto> Diagnoses { get; set; }
        public int TotalCount { get; set; }
    }

    // Treatment DTOs
    public class PatientTreatmentDto
    {
        public int Id { get; set; }
        public string TreatmentName { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpectedCompletionDate { get; set; }
        public int TotalSessions { get; set; }
        public int CompletedSessions { get; set; }
        public TreatmentDoctorDto Doctor { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? BalanceAmount { get; set; }
    }

    public class TreatmentDoctorDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class PaginatedTreatmentsResponseDto
    {
        public List<PatientTreatmentDto> Treatments { get; set; }
        public int TotalCount { get; set; }
    }

    // Image DTOs
    public class PatientImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string ImageType { get; set; }
        public int? ToothNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime UploadedAt { get; set; }
        public ImageUploaderDto UploadedBy { get; set; }
        public bool IsAnalyzedByAI { get; set; }
        public string? FileName { get; set; }
        public long FileSizeBytes { get; set; }
    }

    public class ImageUploaderDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class PaginatedImagesResponseDto
    {
        public List<PatientImageDto> Images { get; set; }
        public int TotalCount { get; set; }
    }

    // Prescription DTOs
    public class PatientPrescriptionDto
    {
        public int Id { get; set; }
        public DateTime IssuedDate { get; set; }
        public string Status { get; set; }
        public PrescriptionDoctorDto Doctor { get; set; }
        public List<PrescriptionMedicationDto> Medications { get; set; }
        public string? Notes { get; set; }
    }

    public class PrescriptionDoctorDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class PrescriptionMedicationDto
    {
        public string MedicationName { get; set; }
        public string Dosage { get; set; }
        public string Frequency { get; set; }
        public string Duration { get; set; }
    }

    public class PaginatedPrescriptionsResponseDto
    {
        public List<PatientPrescriptionDto> Prescriptions { get; set; }
        public int TotalCount { get; set; }
    }

    // Document DTOs
    public class PatientDocumentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string DocumentType { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime UploadedAt { get; set; }
        public DocumentUploaderDto UploadedBy { get; set; }
    }

    public class DocumentUploaderDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class PaginatedDocumentsResponseDto
    {
        public List<PatientDocumentDto> Documents { get; set; }
        public int TotalCount { get; set; }
    }

    // Statistics DTO
    public class PatientStatisticsDto
    {
        public int TotalPatients { get; set; }
        public int NewPatientsThisMonth { get; set; }
        public int ActivePatients { get; set; }
        public int InactivePatients { get; set; }
        public int PatientsWithUpcomingAppointments { get; set; }
    }
}

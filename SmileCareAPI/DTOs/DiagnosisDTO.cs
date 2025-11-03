using SmileCareAPI.Models;

namespace SmileCareAPI.DTOs
{
    public class DIAGDiagnosisListDto
    {
        public int Id { get; set; }
        public string DiagnosisName { get; set; }
        public SeverityLevel Severity { get; set; }
        public DiagnosisStatus Status { get; set; }
        public DateTime DiagnosisDate { get; set; }
        public DIAGPatientBasicDto Patient { get; set; }
        public DIAGDoctorBasicDto Doctor { get; set; }
        public bool IsAIAssisted { get; set; }
        public string? AffectedArea { get; set; }
        public int? ToothNumber { get; set; }
    }

    public class DIAGDiagnosisDetailDto
    {
        public int Id { get; set; }
        public string DiagnosisName { get; set; }
        public string? Description { get; set; }
        public SeverityLevel Severity { get; set; }
        public DiagnosisStatus Status { get; set; }
        public string? AffectedArea { get; set; }
        public int? ToothNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime DiagnosisDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DIAGPatientDetailDto Patient { get; set; }
        public DIAGDoctorDetailDto Doctor { get; set; }
        public DIAGMedicalRecordBasicDto? MedicalRecord { get; set; }
        public DIAGAIResultDto? AIResult { get; set; }
        public List<DIAGTreatmentBasicDto> Treatments { get; set; }
        public List<DIAGImageBasicDto> Images { get; set; }
    }

    public class DIAGPatientBasicDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class DIAGPatientDetailDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
    }

    public class DIAGDoctorBasicDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class DIAGDoctorDetailDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Specialization { get; set; }
    }

    public class DIAGMedicalRecordBasicDto
    {
        public int Id { get; set; }
        public DateTime VisitDate { get; set; }
        public string? ChiefComplaint { get; set; }
    }

    public class DIAGAIResultDto
    {
        public int Id { get; set; }
        public DiagnosisType PrimaryResult { get; set; }
        public decimal ConfidenceScore { get; set; }
        public decimal HealthyProbability { get; set; }
        public decimal CavityProbability { get; set; }
        public decimal GumDiseaseProbability { get; set; }
        public string? HeatmapImageUrl { get; set; }
        public bool IsConfirmedByDoctor { get; set; }
        public bool DoctorAgreement { get; set; }
    }

    public class DIAGTreatmentBasicDto
    {
        public int Id { get; set; }
        public string TreatmentName { get; set; }
        public TreatmentStatus Status { get; set; }
    }

    public class DIAGImageBasicDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public ImageType ImageType { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class DIAGDiagnosisListResponseDto
    {
        public List<DIAGDiagnosisListDto> Diagnoses { get; set; }
        public int TotalCount { get; set; }
    }

    public class DIAGStatisticsDto
    {
        public int TotalDiagnoses { get; set; }
        public int ActiveConditions { get; set; }
        public int ResolvedConditions { get; set; }
        public List<DIAGDiagnosisDistributionDto> DiagnosisDistribution { get; set; }
        public DIAGSeverityDistributionDto SeverityDistribution { get; set; }
        public int AIAssistedCount { get; set; }
        public int ManualCount { get; set; }
    }

    public class DIAGDiagnosisDistributionDto
    {
        public string DiagnosisName { get; set; }
        public int Count { get; set; }
    }

    public class DIAGSeverityDistributionDto
    {
        public int Mild { get; set; }
        public int Moderate { get; set; }
        public int Severe { get; set; }
    }

    // Request DTOs
    public class DIAGCreateDiagnosisDto
    {
        public string PatientId { get; set; }
        public string DiagnosisName { get; set; }
        public string? Description { get; set; }
        public SeverityLevel Severity { get; set; }
        public string? AffectedArea { get; set; }
        public int? ToothNumber { get; set; }
        public int? MedicalRecordId { get; set; }
        public string? Notes { get; set; }
        public bool IsAIAssisted { get; set; }
        public int? AIResultId { get; set; }
    }

    public class DIAGUpdateDiagnosisDto
    {
        public string? DiagnosisName { get; set; }
        public string? Description { get; set; }
        public SeverityLevel? Severity { get; set; }
        public DiagnosisStatus? Status { get; set; }
        public string? Notes { get; set; }
    }

    public class DIAGUpdateStatusDto
    {
        public DiagnosisStatus Status { get; set; }
    }

    public class DIAGCreateResponseDto
    {
        public string Message { get; set; }
        public int DiagnosisId { get; set; }
    }

    public class DIAGMessageResponseDto
    {
        public string Message { get; set; }
    }
}

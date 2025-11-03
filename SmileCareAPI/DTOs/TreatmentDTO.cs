using SmileCareAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.DTOs
{
    // Response DTOs
    public class TRTreatmentListResponseDto
    {
        public int Id { get; set; }
        public string TreatmentName { get; set; }
        public TreatmentStatus Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpectedCompletionDate { get; set; }
        public int TotalSessions { get; set; }
        public int CompletedSessions { get; set; }
        public TRPatientBasicDto Patient { get; set; }
        public TRDoctorBasicDto Doctor { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? BalanceAmount { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
    }
    public class TreatmentImagesUploadDto
    {
        public IFormFile? BeforeImage { get; set; }
        public IFormFile? AfterImage { get; set; }
    }

    public class TRTreatmentDetailsResponseDto
    {
        public int Id { get; set; }
        public string TreatmentName { get; set; }
        public string? Description { get; set; }
        public TreatmentStatus Status { get; set; }
        public int? ToothNumber { get; set; }
        public string? AffectedArea { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpectedCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public int TotalSessions { get; set; }
        public int CompletedSessions { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? BalanceAmount { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string? ProgressNotes { get; set; }
        public string? Complications { get; set; }
        public string? PatientFeedback { get; set; }
        public string? BeforeImageUrl { get; set; }
        public string? AfterImageUrl { get; set; }
        public TRPatientInfoDto Patient { get; set; }
        public TRDoctorInfoDto Doctor { get; set; }
        public TRDiagnosisBasicDto? Diagnosis { get; set; }
        public TRMedicalRecordBasicDto? MedicalRecord { get; set; }
        public List<TRTreatmentSessionDto> Sessions { get; set; }
        public List<TRPrescriptionBasicDto> Prescriptions { get; set; }
        public List<TRImageBasicDto> Images { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class TRTreatmentListDto
    {
        public List<TRTreatmentListResponseDto> Treatments { get; set; }
        public int TotalCount { get; set; }
    }

    public class TRTreatmentSessionDto
    {
        public int Id { get; set; }
        public int SessionNumber { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool IsCompleted { get; set; }
        public string? Notes { get; set; }
        public decimal? SessionCost { get; set; }
    }

    public class TRTreatmentStatisticsDto
    {
        public int TotalTreatments { get; set; }
        public int Planned { get; set; }
        public int Ongoing { get; set; }
        public int Completed { get; set; }
        public int OnHold { get; set; }
        public int Cancelled { get; set; }
        public decimal CompletionRate { get; set; }
        public List<TRTreatmentTypeCountDto> TreatmentsByType { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PendingPayments { get; set; }
    }

    public class TRTreatmentTypeCountDto
    {
        public string TreatmentName { get; set; }
        public int Count { get; set; }
    }

    // Request DTOs
    public class TRCreateTreatmentRequestDto
    {
        [Required]
        public string PatientId { get; set; }

        public int? DiagnosisId { get; set; }

        [Required]
        [MaxLength(200)]
        public string TreatmentName { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int? ToothNumber { get; set; }

        [MaxLength(100)]
        public string? AffectedArea { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? ExpectedCompletionDate { get; set; }

        public int TotalSessions { get; set; } = 1;

        public decimal? EstimatedCost { get; set; }

        [MaxLength(2000)]
        public string? ProgressNotes { get; set; }
    }

    public class TRUpdateTreatmentRequestDto
    {
        [MaxLength(200)]
        public string? TreatmentName { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public TreatmentStatus? Status { get; set; }

        public DateTime? ExpectedCompletionDate { get; set; }

        [MaxLength(2000)]
        public string? ProgressNotes { get; set; }

        [MaxLength(1000)]
        public string? Complications { get; set; }

        [MaxLength(1000)]
        public string? PatientFeedback { get; set; }
    }

    public class TRUpdateTreatmentStatusRequestDto
    {
        [Required]
        public TreatmentStatus Status { get; set; }
    }

    public class TRCreateTreatmentSessionRequestDto
    {
        [Required]
        public int SessionNumber { get; set; }

        public DateTime? ScheduledDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public decimal? SessionCost { get; set; }
    }

    public class TRCompleteTreatmentSessionRequestDto
    {
        [Required]
        public DateTime CompletedDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class TRUpdatePaymentRequestDto
    {
        [Required]
        public decimal PaidAmount { get; set; }

        public PaymentStatus? PaymentStatus { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }
    }

    // Nested DTOs
    public class TRPatientBasicDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class TRPatientInfoDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class TRDoctorBasicDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class TRDoctorInfoDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Specialization { get; set; }
    }

    public class TRDiagnosisBasicDto
    {
        public int Id { get; set; }
        public string DiagnosisName { get; set; }
        public SeverityLevel Severity { get; set; }
    }

    public class TRMedicalRecordBasicDto
    {
        public int Id { get; set; }
        public DateTime VisitDate { get; set; }
    }

    public class TRPrescriptionBasicDto
    {
        public int Id { get; set; }
        public DateTime IssuedDate { get; set; }
        public PrescriptionStatus Status { get; set; }
    }

    public class TRImageBasicDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}

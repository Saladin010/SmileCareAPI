using SmileCareAPI.DTOs;
using SmileCareAPI.Models;

namespace SmileCareAPI.Repositories.Interface
{
    public interface ITreatmentRepository
    {
        Task<(List<Treatment> treatments, int totalCount)> GetAllTreatmentsAsync(
            string? patientId,
            string? doctorId,
            TreatmentStatus? status,
            int page,
            int pageSize);

        Task<Treatment?> GetTreatmentByIdAsync(int id);

        Task<Treatment> CreateTreatmentAsync(Treatment treatment);

        Task<bool> UpdateTreatmentAsync(Treatment treatment);

        Task<bool> DeleteTreatmentAsync(int id);

        Task<bool> UpdateTreatmentStatusAsync(int id, TreatmentStatus status);

        Task<TreatmentSession> CreateTreatmentSessionAsync(TreatmentSession session);

        Task<TreatmentSession?> GetTreatmentSessionByIdAsync(int treatmentId, int sessionId);

        Task<bool> CompleteTreatmentSessionAsync(int treatmentId, int sessionId, DateTime completedDate, string? notes);

        Task<bool> UpdateTreatmentImagesAsync(int id, string? beforeImageUrl, string? afterImageUrl);

        Task<bool> UpdateTreatmentPaymentAsync(int id, decimal paidAmount, PaymentStatus? paymentStatus);

        Task<TRTreatmentStatisticsDto> GetTreatmentStatisticsAsync(
            string? doctorId,
            DateTime? startDate,
            DateTime? endDate);

        Task<bool> TreatmentExistsAsync(int id);

        Task<bool> IsPatientOwnerAsync(int treatmentId, string patientId);

        Task<bool> IsDoctorAssignedAsync(int treatmentId, string doctorId);

        Task<int> GetCompletedSessionsCountAsync(int treatmentId);

        Task<decimal> GetTotalPaidAmountAsync(int treatmentId);
    }
}

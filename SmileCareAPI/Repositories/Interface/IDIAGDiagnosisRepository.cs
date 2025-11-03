using SmileCareAPI.DTOs;
using SmileCareAPI.Models;

namespace SmileCareAPI.Repositories.Interface
{
    public interface IDIAGDiagnosisRepository
    {
        Task<DIAGDiagnosisListResponseDto> GetAllDiagnosesAsync(
            string? patientId = null,
            string? doctorId = null,
            DiagnosisStatus? status = null,
            bool? isAIAssisted = null,
            int page = 1,
            int pageSize = 10
        );

        Task<DIAGDiagnosisDetailDto?> GetDiagnosisByIdAsync(int id);

        Task<DIAGCreateResponseDto> CreateDiagnosisAsync(DIAGCreateDiagnosisDto dto, string doctorId);

        Task<DIAGMessageResponseDto> UpdateDiagnosisAsync(int id, DIAGUpdateDiagnosisDto dto, string userId);

        Task<DIAGMessageResponseDto> DeleteDiagnosisAsync(int id, string userId);

        Task<DIAGMessageResponseDto> UpdateDiagnosisStatusAsync(int id, DIAGUpdateStatusDto dto, string userId);

        Task<DIAGStatisticsDto> GetDiagnosisStatisticsAsync(
            string? doctorId = null,
            DateTime? startDate = null,
            DateTime? endDate = null
        );

        Task<bool> DiagnosisExistsAsync(int id);

        Task<bool> CanUserAccessDiagnosisAsync(int diagnosisId, string userId, UserRole userRole);
    }
}

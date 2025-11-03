using SmileCareAPI.DTOs;

namespace SmileCareAPI.Repositories.Interface
{
    public interface IPatientsRepository
    {
        // Main CRUD Operations
        Task<PaginatedPatientsResponseDtoP> GetAllPatientsAsync(PatientFilterDto filter);
        Task<PatientDetailsResponseDto> GetPatientByIdAsync(string patientId);
        Task<(bool Success, string Message, string PatientId)> CreatePatientAsync(CreatePatientDto dto);
        Task<(bool Success, string Message)> UpdatePatientAsync(string patientId, UpdatePatientDto dto);
        Task<(bool Success, string Message)> DeletePatientAsync(string patientId);

        // Medical History
        Task<MedicalHistoryDto> GetPatientMedicalHistoryAsync(string patientId);
        Task<(bool Success, string Message)> UpdatePatientMedicalHistoryAsync(string patientId, UpdateMedicalHistoryDto dto);

        // Patient Appointments
        Task<PaginatedAppointmentsResponseDtoP> GetPatientAppointmentsAsync(string patientId, string? status, int page, int pageSize);

        // Medical Records
        Task<PaginatedMedicalRecordsResponseDto> GetPatientMedicalRecordsAsync(string patientId, int page, int pageSize);

        // Diagnoses
        Task<PaginatedDiagnosesResponseDto> GetPatientDiagnosesAsync(string patientId, string? status, int page, int pageSize);

        // Treatments
        Task<PaginatedTreatmentsResponseDto> GetPatientTreatmentsAsync(string patientId, string? status, int page, int pageSize);

        // Images
        Task<PaginatedImagesResponseDto> GetPatientImagesAsync(string patientId, string? imageType, bool? isAnalyzedByAI, int page, int pageSize);

        // Prescriptions
        Task<PaginatedPrescriptionsResponseDto> GetPatientPrescriptionsAsync(string patientId, string? status, int page, int pageSize);

        // Documents
        Task<PaginatedDocumentsResponseDto> GetPatientDocumentsAsync(string patientId, string? documentType, int page, int pageSize);

        // Assign Doctor
        Task<(bool Success, string Message)> AssignDoctorToPatientAsync(string patientId, string doctorId);

        // Statistics
        Task<PatientStatisticsDto> GetPatientsStatisticsAsync();
    }
}

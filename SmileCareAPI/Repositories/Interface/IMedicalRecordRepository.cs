using SmileCareAPI.Models;

namespace SmileCareAPI.Repositories.Interface
{
    public interface IMedicalRecordRepository
    {
        Task<(IEnumerable<MedicalRecord> Records, int TotalCount)> GetAllMedicalRecordsAsync(
            string? patientId,
            string? doctorId,
            DateTime? startDate,
            DateTime? endDate,
            int page,
            int pageSize);

        Task<MedicalRecord?> GetMedicalRecordByIdAsync(int id);

        Task<(IEnumerable<MedicalRecord> Records, int TotalCount)> GetMedicalRecordsByPatientIdAsync(
            string patientId,
            int page,
            int pageSize);

        Task<MedicalRecord> CreateMedicalRecordAsync(MedicalRecord medicalRecord);

        Task<bool> UpdateMedicalRecordAsync(MedicalRecord medicalRecord);

        Task<bool> DeleteMedicalRecordAsync(int id);

        Task<bool> MedicalRecordExistsAsync(int id);

        Task<bool> PatientExistsAsync(string patientId);

        Task<bool> AppointmentExistsAsync(int appointmentId);

        Task<bool> IsAppointmentAlreadyLinkedAsync(int appointmentId);
    }
}

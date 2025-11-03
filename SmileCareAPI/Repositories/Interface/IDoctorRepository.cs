using SmileCareAPI.DTOs;
using SmileCareAPI.Models;

namespace SmileCareAPI.Repositories.Interface
{
    public interface IDoctorRepository
    {
        Task<(List<DoctorListDto> Doctors, int TotalCount)> GetAllDoctorsAsync(
            string? search,
            string? specialization,
            int page,
            int pageSize);

        Task<DoctorDetailsDto?> GetDoctorByIdAsync(string doctorId);

        Task<Doctor?> GetDoctorEntityAsync(string doctorId);

        Task<(List<PatientListDto> Patients, int TotalCount)> GetDoctorPatientsAsync(
            string doctorId,
            int page,
            int pageSize);

        Task<(List<AppointmentListDto> Appointments, int TotalCount)> GetDoctorAppointmentsAsync(
            string doctorId,
            AppointmentStatus? status,
            DateTime? date,
            int page,
            int pageSize);

        Task<List<WorkingHoursDto>> GetDoctorWorkingHoursAsync(string doctorId);

        Task<bool> UpdateDoctorWorkingHoursAsync(string doctorId, List<UpdateWorkingHoursDto> workingHours);

        Task<List<AvailableSlotDto>> GetDoctorAvailableSlotsAsync(string doctorId, DateTime date);

        Task<bool> AddDoctorHolidayAsync(string doctorId, DateTime holidayDate, string holidayName);

        Task<List<DoctorHolidayDto>> GetDoctorHolidaysAsync(string doctorId);

        Task<bool> DeleteDoctorHolidayAsync(int holidayId, string doctorId);

        Task<DoctorStatisticsDto> GetDoctorStatisticsAsync(string doctorId);

        Task<bool> DoctorExistsAsync(string doctorId);
    }
}

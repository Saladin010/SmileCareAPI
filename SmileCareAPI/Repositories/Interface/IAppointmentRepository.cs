using SmileCareAPI.DTOs;
using SmileCareAPI.Models;

namespace SmileCareAPI.Repositories.Interface
{
    public interface IAppointmentRepository
    {
        // CRUD Operations
        Task<AppointmentListResponseAPPDto> GetAllAppointmentsAsync(
            AppointmentQueryParametersAPPDto query,
            string currentUserId,
            UserRole currentUserRole);

        Task<AppointmentDetailAPPDto?> GetAppointmentByIdAsync(
            int id,
            string currentUserId,
            UserRole currentUserRole);

        Task<int> CreateAppointmentAsync(
            CreateAppointmentAPPDto dto,
            string createdBy);

        Task<bool> UpdateAppointmentAsync(
            int id,
            UpdateAppointmentAPPDto dto,
            string currentUserId,
            UserRole currentUserRole);

        Task<bool> DeleteAppointmentAsync(
            int id,
            string currentUserId,
            UserRole currentUserRole);

        // Status Operations
        Task<bool> ConfirmAppointmentAsync(
            int id,
            ConfirmAppointmentAPPDto dto,
            string currentUserId,
            UserRole currentUserRole);

        Task<bool> CancelAppointmentAsync(
            int id,
            CancelAppointmentAPPDto dto,
            string currentUserId,
            UserRole currentUserRole);

        Task<bool> CompleteAppointmentAsync(
            int id,
            string currentUserId,
            UserRole currentUserRole);

        Task<bool> MarkAsNoShowAsync(
            int id,
            string currentUserId,
            UserRole currentUserRole);

        Task<bool> RescheduleAppointmentAsync(
            int id,
            RescheduleAppointmentAPPDto dto,
            string currentUserId,
            UserRole currentUserRole);

        Task<bool> SendReminderAsync(
            int id,
            SendReminderAPPDto dto,
            string currentUserId,
            UserRole currentUserRole);

        // Query Operations
        Task<AppointmentListResponseAPPDto> GetTodayAppointmentsAsync(
            string currentUserId,
            UserRole currentUserRole,
            int page = 1,
            int pageSize = 20);

        Task<AppointmentListResponseAPPDto> GetUpcomingAppointmentsAsync(
            UpcomingAppointmentsQueryAPPDto query,
            string currentUserId,
            UserRole currentUserRole);

        Task<AvailableSlotsResponseAPPDto> GetAvailableSlotsAsync(
            AvailableSlotsQueryAPPDto query);

        Task<AppointmentStatisticsAPPDto> GetStatisticsAsync(
            StatisticsQueryParametersAPPDto query,
            string currentUserId,
            UserRole currentUserRole);

        // Validation
        Task<bool> IsSlotAvailableAsync(
            string doctorId,
            DateTime date,
            TimeSpan startTime,
            int durationMinutes,
            int? excludeAppointmentId = null);

        Task<bool> HasConflictingAppointmentAsync(
            string patientId,
            DateTime date,
            TimeSpan startTime,
            int durationMinutes,
            int? excludeAppointmentId = null);

        Task<bool> IsDoctorAvailableAsync(
            string doctorId,
            DateTime date,
            TimeSpan startTime);
    }
}

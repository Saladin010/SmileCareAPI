using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmileCareAPI.DTOs;
using SmileCareAPI.Models;
using SmileCareAPI.Repositories.Interface;

namespace SmileCareAPI.Controllers
{
  

    [Authorize]
    [ApiController]
    [Route("api/appointments")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(
            IAppointmentRepository appointmentRepository,
            ILogger<AppointmentsController> logger)
        {
            _appointmentRepository = appointmentRepository;
            _logger = logger;
        }

        // GET: api/appointments
        [HttpGet]
        public async Task<ActionResult<AppointmentListResponseAPPDto>> GetAllAppointments(
            [FromQuery] AppointmentQueryParametersAPPDto query)
        {
            try
            {
                var (userId, userRole) = GetCurrentUser();
                var result = await _appointmentRepository.GetAllAppointmentsAsync(query, userId, userRole);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments");
                return StatusCode(500, new { message = "An error occurred while retrieving appointments" });
            }
        }

        // GET: api/appointments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDetailAPPDto>> GetAppointmentById(int id)
        {
            try
            {
                var (userId, userRole) = GetCurrentUser();
                var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id, userId, userRole);

                if (appointment == null)
                    return NotFound(new { message = "Appointment not found" });

                return Ok(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointment {AppointmentId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the appointment" });
            }
        }

        // POST: api/appointments
        [HttpPost]
        [Authorize(Roles = "Doctor,Receptionist,Patient")]
        public async Task<ActionResult> CreateAppointment([FromBody] CreateAppointmentAPPDto dto)
        {
            try
            {
                var (userId, userRole) = GetCurrentUser();

                // Validate patient exists
                if (userRole == UserRole.Patient && dto.PatientId != userId)
                    return Forbid();

                // Validate slot availability
                var isSlotAvailable = await _appointmentRepository.IsSlotAvailableAsync(
                    dto.DoctorId,
                    dto.AppointmentDate,
                    dto.StartTime,
                    dto.DurationMinutes);

                if (!isSlotAvailable)
                    return BadRequest(new { message = "The selected time slot is not available" });

                // Check for patient conflicts
                var hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
                    dto.PatientId,
                    dto.AppointmentDate,
                    dto.StartTime,
                    dto.DurationMinutes);

                if (hasConflict)
                    return BadRequest(new { message = "Patient already has an appointment at this time" });

                // Check doctor availability
                var isDoctorAvailable = await _appointmentRepository.IsDoctorAvailableAsync(
                    dto.DoctorId,
                    dto.AppointmentDate,
                    dto.StartTime);

                if (!isDoctorAvailable)
                    return BadRequest(new { message = "Doctor is not available at this time" });

                var appointmentId = await _appointmentRepository.CreateAppointmentAsync(dto, userId);

                return CreatedAtAction(
                    nameof(GetAppointmentById),
                    new { id = appointmentId },
                    new { message = "Appointment created successfully", appointmentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment");
                return StatusCode(500, new { message = "An error occurred while creating the appointment" });
            }
        }

        // PUT: api/appointments/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Doctor,Receptionist")]
        public async Task<ActionResult> UpdateAppointment(int id, [FromBody] UpdateAppointmentAPPDto dto)
        {
            try
            {
                var (userId, userRole) = GetCurrentUser();

                // Validate new slot if date/time changed
                if (dto.AppointmentDate.HasValue || dto.StartTime.HasValue)
                {
                    var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id, userId, userRole);
                    if (appointment == null)
                        return NotFound(new { message = "Appointment not found" });

                    var newDate = dto.AppointmentDate ?? appointment.AppointmentDate;
                    var newStartTime = dto.StartTime ?? appointment.StartTime;
                    var newDuration = dto.DurationMinutes ?? appointment.DurationMinutes;

                    var isSlotAvailable = await _appointmentRepository.IsSlotAvailableAsync(
                        appointment.Doctor.Id,
                        newDate,
                        newStartTime,
                        newDuration,
                        id);

                    if (!isSlotAvailable)
                        return BadRequest(new { message = "The selected time slot is not available" });
                }

                var success = await _appointmentRepository.UpdateAppointmentAsync(id, dto, userId, userRole);

                if (!success)
                    return NotFound(new { message = "Appointment not found or cannot be updated" });

                return Ok(new { message = "Appointment updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment {AppointmentId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the appointment" });
            }
        }

        // PATCH: api/appointments/{id}/confirm
        [HttpPatch("{id}/confirm")]
        [Authorize(Roles = "Doctor,Receptionist")]
        public async Task<ActionResult> ConfirmAppointment(int id, [FromBody] ConfirmAppointmentAPPDto dto)
        {
            try
            {
                var (userId, userRole) = GetCurrentUser();
                var success = await _appointmentRepository.ConfirmAppointmentAsync(id, dto, userId, userRole);

                if (!success)
                    return NotFound(new { message = "Appointment not found or cannot be confirmed" });

                return Ok(new { message = "Appointment confirmed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming appointment {AppointmentId}", id);
                return StatusCode(500, new { message = "An error occurred while confirming the appointment" });
            }
        }

        // PATCH: api/appointments/{id}/cancel
        [HttpPatch("{id}/cancel")]
        public async Task<ActionResult> CancelAppointment(int id, [FromBody] CancelAppointmentAPPDto dto)
        {
            try
            {
                var (userId, userRole) = GetCurrentUser();
                var success = await _appointmentRepository.CancelAppointmentAsync(id, dto, userId, userRole);

                if (!success)
                    return NotFound(new { message = "Appointment not found or cannot be cancelled" });

                return Ok(new { message = "Appointment cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling appointment {AppointmentId}", id);
                return StatusCode(500, new { message = "An error occurred while cancelling the appointment" });
            }
        }

        // PATCH: api/appointments/{id}/complete
        [HttpPatch("{id}/complete")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult> CompleteAppointment(int id)
        {
            try
            {
                var (userId, userRole) = GetCurrentUser();
                var success = await _appointmentRepository.CompleteAppointmentAsync(id, userId, userRole);

                if (!success)
                    return NotFound(new { message = "Appointment not found or cannot be completed" });

                return Ok(new { message = "Appointment completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing appointment {AppointmentId}", id);
                return StatusCode(500, new { message = "An error occurred while completing the appointment" });
            }
        }

        // PATCH: api/appointments/{id}/no-show
        [HttpPatch("{id}/no-show")]
        [Authorize(Roles = "Doctor,Receptionist")]
        public async Task<ActionResult> MarkAsNoShow(int id)
        {
            try
            {
                var (userId, userRole) = GetCurrentUser();
                var success = await _appointmentRepository.MarkAsNoShowAsync(id, userId, userRole);

                if (!success)
                    return NotFound(new { message = "Appointment not found or cannot be marked as no-show" });

                return Ok(new { message = "Appointment marked as no-show successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking appointment as no-show {AppointmentId}", id);
                return StatusCode(500, new { message = "An error occurred while marking the appointment" });
            }
        }

        // POST: api/appointments/{id}/reschedule
        [HttpPost("{id}/reschedule")]
        public async Task<ActionResult> RescheduleAppointment(int id, [FromBody] RescheduleAppointmentAPPDto dto)
        {
            try
            {
                var (userId, userRole) = GetCurrentUser();

                // Get appointment to check doctor
                var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id, userId, userRole);
                if (appointment == null)
                    return NotFound(new { message = "Appointment not found" });

                // Validate new slot availability
                var isSlotAvailable = await _appointmentRepository.IsSlotAvailableAsync(
                    appointment.Doctor.Id,
                    dto.NewDate,
                    dto.NewStartTime,
                    appointment.DurationMinutes,
                    id);

                if (!isSlotAvailable)
                    return BadRequest(new { message = "The selected time slot is not available" });

                var success = await _appointmentRepository.RescheduleAppointmentAsync(id, dto, userId, userRole);

                if (!success)
                    return NotFound(new { message = "Appointment not found or cannot be rescheduled" });

                return Ok(new { message = "Appointment rescheduled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rescheduling appointment {AppointmentId}", id);
                return StatusCode(500, new { message = "An error occurred while rescheduling the appointment" });
            }
        }

        // POST: api/appointments/{id}/send-reminder
        [HttpPost("{id}/send-reminder")]
        [Authorize(Roles = "Doctor,Receptionist")]
        public async Task<ActionResult> SendReminder(int id, [FromBody] SendReminderAPPDto dto)
        {
            try
            {
                var (userId, userRole) = GetCurrentUser();
                var success = await _appointmentRepository.SendReminderAsync(id, dto, userId, userRole);

                if (!success)
                    return NotFound(new { message = "Appointment not found" });

                return Ok(new { message = "Reminder sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending reminder for appointment {AppointmentId}", id);
                return StatusCode(500, new { message = "An error occurred while sending the reminder" });
            }
        }

        // GET: api/appointments/statistics
        [HttpGet("statistics")]
        [Authorize(Roles = "Doctor,Receptionist")]
        public async Task<ActionResult<AppointmentStatisticsAPPDto>> GetStatistics(
            [FromQuery] StatisticsQueryParametersAPPDto query)
        {
            try
            {
                var (userId, userRole) = GetCurrentUser();
                var statistics = await _appointmentRepository.GetStatisticsAsync(query, userId, userRole);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointment statistics");
                return StatusCode(500, new { message = "An error occurred while retrieving statistics" });
            }
        }

        // GET: api/appointments/today
        [HttpGet("today")]
        [Authorize(Roles = "Doctor,Receptionist")]
        public async Task<ActionResult<AppointmentListResponseAPPDto>> GetTodayAppointments(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var (userId, userRole) = GetCurrentUser();
                var result = await _appointmentRepository.GetTodayAppointmentsAsync(userId, userRole, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving today's appointments");
                return StatusCode(500, new { message = "An error occurred while retrieving today's appointments" });
            }
        }

        // GET: api/appointments/upcoming
        [HttpGet("upcoming")]
        public async Task<ActionResult<AppointmentListResponseAPPDto>> GetUpcomingAppointments(
            [FromQuery] UpcomingAppointmentsQueryAPPDto query)
        {
            try
            {
                var (userId, userRole) = GetCurrentUser();
                var result = await _appointmentRepository.GetUpcomingAppointmentsAsync(query, userId, userRole);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming appointments");
                return StatusCode(500, new { message = "An error occurred while retrieving upcoming appointments" });
            }
        }

        // GET: api/appointments/available-slots
        [HttpGet("available-slots")]
        public async Task<ActionResult<AvailableSlotsResponseAPPDto>> GetAvailableSlots(
            [FromQuery] AvailableSlotsQueryAPPDto query)
        {
            try
            {
                var result = await _appointmentRepository.GetAvailableSlotsAsync(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available slots");
                return StatusCode(500, new { message = "An error occurred while retrieving available slots" });
            }
        }

        private (string userId, UserRole userRole) GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User not authenticated");

            var roleString = User.FindFirstValue(ClaimTypes.Role)
                ?? throw new UnauthorizedAccessException("User role not found");

            var userRole = Enum.Parse<UserRole>(roleString);

            return (userId, userRole);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmileCareAPI.DTOs;
using SmileCareAPI.Models;
using SmileCareAPI.Repositories.Interface;

namespace SmileCareAPI.Controllers
{
  
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorRepository _doctorRepository;

        public DoctorsController(IDoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        /// <summary>
        /// Get all doctors with filtering and search
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedDoctorsResponseDto>> GetAllDoctors(
            [FromQuery] string? search,
            [FromQuery] string? specialization,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                var (doctors, totalCount) = await _doctorRepository.GetAllDoctorsAsync(
                    search,
                    specialization,
                    page,
                    pageSize);

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var response = new PaginatedDoctorsResponseDto
                {
                    Doctors = doctors,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    TotalPages = totalPages
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching doctors", error = ex.Message });
            }
        }

        /// <summary>
        /// Get doctor details by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<DoctorDetailsDto>> GetDoctorById(string id)
        {
            try
            {
                var doctor = await _doctorRepository.GetDoctorByIdAsync(id);

                if (doctor == null)
                {
                    return NotFound(new { message = "Doctor not found" });
                }

                return Ok(doctor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching doctor details", error = ex.Message });
            }
        }

        /// <summary>
        /// Get patients assigned to a specific doctor
        /// </summary>
        [HttpGet("{id}/patients")]
        public async Task<ActionResult<PaginatedPatientsResponseDto>> GetDoctorPatients(
            string id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (!await _doctorRepository.DoctorExistsAsync(id))
                {
                    return NotFound(new { message = "Doctor not found" });
                }

                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                var (patients, totalCount) = await _doctorRepository.GetDoctorPatientsAsync(
                    id,
                    page,
                    pageSize);

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var response = new PaginatedPatientsResponseDto
                {
                    Patients = patients,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    TotalPages = totalPages
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching doctor's patients", error = ex.Message });
            }
        }

        /// <summary>
        /// Get appointments for a specific doctor
        /// </summary>
        [HttpGet("{id}/appointments")]
        public async Task<ActionResult<PaginatedAppointmentsResponseDto>> GetDoctorAppointments(
            string id,
            [FromQuery] AppointmentStatus? status,
            [FromQuery] DateTime? date,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (!await _doctorRepository.DoctorExistsAsync(id))
                {
                    return NotFound(new { message = "Doctor not found" });
                }

                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                var (appointments, totalCount) = await _doctorRepository.GetDoctorAppointmentsAsync(
                    id,
                    status,
                    date,
                    page,
                    pageSize);

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var response = new PaginatedAppointmentsResponseDto
                {
                    Appointments = appointments,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    TotalPages = totalPages
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching doctor's appointments", error = ex.Message });
            }
        }

        /// <summary>
        /// Get working hours for a specific doctor
        /// </summary>
        [HttpGet("{id}/working-hours")]
        public async Task<ActionResult<List<WorkingHoursDto>>> GetDoctorWorkingHours(string id)
        {
            try
            {
                if (!await _doctorRepository.DoctorExistsAsync(id))
                {
                    return NotFound(new { message = "Doctor not found" });
                }

                var workingHours = await _doctorRepository.GetDoctorWorkingHoursAsync(id);

                return Ok(workingHours);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching working hours", error = ex.Message });
            }
        }

        /// <summary>
        /// Update working hours for a specific doctor
        /// </summary>
        [HttpPut("{id}/working-hours")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> UpdateDoctorWorkingHours(
            string id,
            [FromBody] List<UpdateWorkingHoursDto> workingHours)
        {
            try
            {
                if (!await _doctorRepository.DoctorExistsAsync(id))
                {
                    return NotFound(new { message = "Doctor not found" });
                }

                if (workingHours == null || !workingHours.Any())
                {
                    return BadRequest(new { message = "Working hours data is required" });
                }

                // Validate that all days of week are provided
                var daysProvided = workingHours.Select(wh => wh.DayOfWeek).Distinct().Count();
                if (daysProvided != 7)
                {
                    return BadRequest(new { message = "All 7 days of the week must be provided" });
                }

                var result = await _doctorRepository.UpdateDoctorWorkingHoursAsync(id, workingHours);

                if (result)
                {
                    return Ok(new { message = "Working hours updated successfully" });
                }

                return StatusCode(500, new { message = "Failed to update working hours" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating working hours", error = ex.Message });
            }
        }

        /// <summary>
        /// Get available time slots for a doctor on a specific date
        /// </summary>
        [HttpGet("{id}/available-slots")]
        public async Task<ActionResult<AvailableSlotsResponseDto>> GetDoctorAvailableSlots(
            string id,
            [FromQuery] DateTime date)
        {
            try
            {
                if (!await _doctorRepository.DoctorExistsAsync(id))
                {
                    return NotFound(new { message = "Doctor not found" });
                }

                if (date.Date < DateTime.UtcNow.Date)
                {
                    return BadRequest(new { message = "Cannot get available slots for past dates" });
                }

                var availableSlots = await _doctorRepository.GetDoctorAvailableSlotsAsync(id, date);

                var response = new AvailableSlotsResponseDto
                {
                    Date = date,
                    AvailableSlots = availableSlots
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching available slots", error = ex.Message });
            }
        }

        /// <summary>
        /// Add a holiday for a specific doctor
        /// </summary>
        [HttpPost("{id}/holidays")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> AddDoctorHoliday(
            string id,
            [FromBody] AddHolidayDto holidayDto)
        {
            try
            {
                if (!await _doctorRepository.DoctorExistsAsync(id))
                {
                    return NotFound(new { message = "Doctor not found" });
                }

                if (holidayDto.HolidayDate.Date < DateTime.UtcNow.Date)
                {
                    return BadRequest(new { message = "Cannot add holiday for past dates" });
                }

                var result = await _doctorRepository.AddDoctorHolidayAsync(
                    id,
                    holidayDto.HolidayDate,
                    holidayDto.HolidayName);

                if (result)
                {
                    return Ok(new { message = "Holiday added successfully" });
                }

                return StatusCode(500, new { message = "Failed to add holiday" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding holiday", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all holidays for a specific doctor
        /// </summary>
        [HttpGet("{id}/holidays")]
        public async Task<ActionResult<List<DoctorHolidayDto>>> GetDoctorHolidays(string id)
        {
            try
            {
                if (!await _doctorRepository.DoctorExistsAsync(id))
                {
                    return NotFound(new { message = "Doctor not found" });
                }

                var holidays = await _doctorRepository.GetDoctorHolidaysAsync(id);

                return Ok(holidays);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching holidays", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a specific holiday
        /// </summary>
        [HttpDelete("holidays/{holidayId}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> DeleteDoctorHoliday(int holidayId, [FromQuery] string doctorId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(doctorId))
                {
                    return BadRequest(new { message = "Doctor ID is required" });
                }

                if (!await _doctorRepository.DoctorExistsAsync(doctorId))
                {
                    return NotFound(new { message = "Doctor not found" });
                }

                var result = await _doctorRepository.DeleteDoctorHolidayAsync(holidayId, doctorId);

                if (result)
                {
                    return Ok(new { message = "Holiday deleted successfully" });
                }

                return NotFound(new { message = "Holiday not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting holiday", error = ex.Message });
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmileCareAPI.DTOs;
using SmileCareAPI.Models;
using SmileCareAPI.Repositories.Interface;

namespace SmileCareAPI.Controllers
{
  
    [ApiController]
    [Route("api/diagnoses")]
    [Authorize]
    public class DiagnosesController : ControllerBase
    {
        private readonly IDIAGDiagnosisRepository _diagnosisRepository;
        private readonly ILogger<DiagnosesController> _logger;

        public DiagnosesController(
            IDIAGDiagnosisRepository diagnosisRepository,
            ILogger<DiagnosesController> logger)
        {
            _diagnosisRepository = diagnosisRepository;
            _logger = logger;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        private UserRole GetUserRole() => Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);

        /// <summary>
        /// Get all diagnoses with optional filters
        /// </summary>
        /// <param name="patientId">Filter by patient ID</param>
        /// <param name="doctorId">Filter by doctor ID</param>
        /// <param name="status">Filter by diagnosis status</param>
        /// <param name="isAIAssisted">Filter by AI assistance</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        [HttpGet]
        public async Task<ActionResult<DIAGDiagnosisListResponseDto>> GetAllDiagnoses(
            [FromQuery] string? patientId = null,
            [FromQuery] string? doctorId = null,
            [FromQuery] DiagnosisStatus? status = null,
            [FromQuery] bool? isAIAssisted = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetUserId();
                var userRole = GetUserRole();

                // Apply role-based filtering
                if (userRole == UserRole.Patient)
                {
                    patientId = userId;
                }
                else if (userRole == UserRole.Doctor && string.IsNullOrEmpty(doctorId))
                {
                    doctorId = userId;
                }

                var result = await _diagnosisRepository.GetAllDiagnosesAsync(
                    patientId, doctorId, status, isAIAssisted, page, pageSize);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting diagnoses");
                return StatusCode(500, new { message = "An error occurred while retrieving diagnoses" });
            }
        }

        /// <summary>
        /// Get diagnosis details by ID
        /// </summary>
        /// <param name="id">Diagnosis ID</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<DIAGDiagnosisDetailDto>> GetDiagnosisById(int id)
        {
            try
            {
                var userId = GetUserId();
                var userRole = GetUserRole();

                // Check access permissions
                var canAccess = await _diagnosisRepository.CanUserAccessDiagnosisAsync(id, userId, userRole);
                if (!canAccess)
                    return Forbid();

                var diagnosis = await _diagnosisRepository.GetDiagnosisByIdAsync(id);

                if (diagnosis == null)
                    return NotFound(new { message = "Diagnosis not found" });

                return Ok(diagnosis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting diagnosis {DiagnosisId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving diagnosis details" });
            }
        }

        /// <summary>
        /// Create a new diagnosis
        /// </summary>
        /// <param name="dto">Diagnosis creation data</param>
        [HttpPost]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<ActionResult<DIAGCreateResponseDto>> CreateDiagnosis([FromBody] DIAGCreateDiagnosisDto dto)
        {
            try
            {
                var userId = GetUserId();
                var userRole = GetUserRole();

                // Doctors can only create diagnoses for themselves
                string doctorId = userRole == UserRole.Doctor ? userId : dto.PatientId;

                var result = await _diagnosisRepository.CreateDiagnosisAsync(dto, doctorId);

                return CreatedAtAction(
                    nameof(GetDiagnosisById),
                    new { id = result.DiagnosisId },
                    result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating diagnosis");
                return StatusCode(500, new { message = "An error occurred while creating the diagnosis" });
            }
        }

        /// <summary>
        /// Update diagnosis information
        /// </summary>
        /// <param name="id">Diagnosis ID</param>
        /// <param name="dto">Update data</param>
        [HttpPut("{id}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<ActionResult<DIAGMessageResponseDto>> UpdateDiagnosis(
            int id,
            [FromBody] DIAGUpdateDiagnosisDto dto)
        {
            try
            {
                var userId = GetUserId();
                var userRole = GetUserRole();

                // Check access permissions
                var canAccess = await _diagnosisRepository.CanUserAccessDiagnosisAsync(id, userId, userRole);
                if (!canAccess)
                    return Forbid();

                var result = await _diagnosisRepository.UpdateDiagnosisAsync(id, dto, userId);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating diagnosis {DiagnosisId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the diagnosis" });
            }
        }

        /// <summary>
        /// Delete a diagnosis
        /// </summary>
        /// <param name="id">Diagnosis ID</param>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<ActionResult<DIAGMessageResponseDto>> DeleteDiagnosis(int id)
        {
            try
            {
                var userId = GetUserId();
                var userRole = GetUserRole();

                // Check access permissions
                var canAccess = await _diagnosisRepository.CanUserAccessDiagnosisAsync(id, userId, userRole);
                if (!canAccess)
                    return Forbid();

                var result = await _diagnosisRepository.DeleteDiagnosisAsync(id, userId);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting diagnosis {DiagnosisId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the diagnosis" });
            }
        }

        /// <summary>
        /// Update diagnosis status
        /// </summary>
        /// <param name="id">Diagnosis ID</param>
        /// <param name="dto">Status update data</param>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<ActionResult<DIAGMessageResponseDto>> UpdateDiagnosisStatus(
            int id,
            [FromBody] DIAGUpdateStatusDto dto)
        {
            try
            {
                var userId = GetUserId();
                var userRole = GetUserRole();

                // Check access permissions
                var canAccess = await _diagnosisRepository.CanUserAccessDiagnosisAsync(id, userId, userRole);
                if (!canAccess)
                    return Forbid();

                var result = await _diagnosisRepository.UpdateDiagnosisStatusAsync(id, dto, userId);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating diagnosis status {DiagnosisId}", id);
                return StatusCode(500, new { message = "An error occurred while updating diagnosis status" });
            }
        }

        /// <summary>
        /// Get diagnosis statistics
        /// </summary>
        /// <param name="doctorId">Filter by doctor ID</param>
        /// <param name="startDate">Filter by start date</param>
        /// <param name="endDate">Filter by end date</param>
        [HttpGet("statistics")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<ActionResult<DIAGStatisticsDto>> GetDiagnosisStatistics(
            [FromQuery] string? doctorId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = GetUserId();
                var userRole = GetUserRole();

                // Doctors can only view their own statistics
                if (userRole == UserRole.Doctor)
                {
                    doctorId = userId;
                }

                var statistics = await _diagnosisRepository.GetDiagnosisStatisticsAsync(
                    doctorId, startDate, endDate);

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting diagnosis statistics");
                return StatusCode(500, new { message = "An error occurred while retrieving statistics" });
            }
        }
    }
}

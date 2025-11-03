using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmileCareAPI.DTOs;
using SmileCareAPI.Repositories.Interface;

namespace SmileCareAPI.Controllers
{
   
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientsRepository _patientsRepository;

        public PatientsController(IPatientsRepository patientsRepository)
        {
            _patientsRepository = patientsRepository;
        }

        /// <summary>
        /// Get all patients with filtering and pagination
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor,Receptionist")]
        public async Task<ActionResult<PaginatedPatientsResponseDtoP>> GetAllPatients([FromQuery] PatientFilterDto filter)
        {
            try
            {
                var result = await _patientsRepository.GetAllPatientsAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving patients", error = ex.Message });
            }
        }

        /// <summary>
        /// Get patient details by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PatientDetailsResponseDto>> GetPatientById(string id)
        {
            try
            {
                var result = await _patientsRepository.GetPatientByIdAsync(id);

                if (result == null)
                    return NotFound(new { message = "Patient not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving patient details", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new patient
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<ActionResult> CreatePatient([FromBody] CreatePatientDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var (success, message, patientId) = await _patientsRepository.CreatePatientAsync(dto);

                if (!success)
                    return BadRequest(new { message });

                return CreatedAtAction(nameof(GetPatientById), new { id = patientId }, new { message, patientId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the patient", error = ex.Message });
            }
        }

        /// <summary>
        /// Update patient information
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<ActionResult> UpdatePatient(string id, [FromBody] UpdatePatientDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var (success, message) = await _patientsRepository.UpdatePatientAsync(id, dto);

                if (!success)
                    return NotFound(new { message });

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the patient", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a patient
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeletePatient(string id)
        {
            try
            {
                var (success, message) = await _patientsRepository.DeletePatientAsync(id);

                if (!success)
                    return NotFound(new { message });

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the patient", error = ex.Message });
            }
        }

        /// <summary>
        /// Get patient medical history
        /// </summary>
        [HttpGet("{id}/medical-history")]
        public async Task<ActionResult<MedicalHistoryDto>> GetPatientMedicalHistory(string id)
        {
            try
            {
                var result = await _patientsRepository.GetPatientMedicalHistoryAsync(id);

                if (result == null)
                    return NotFound(new { message = "Medical history not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving medical history", error = ex.Message });
            }
        }

        /// <summary>
        /// Update patient medical history
        /// </summary>
        [HttpPut("{id}/medical-history")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult> UpdatePatientMedicalHistory(string id, [FromBody] UpdateMedicalHistoryDto dto)
        {
            try
            {
                var (success, message) = await _patientsRepository.UpdatePatientMedicalHistoryAsync(id, dto);

                if (!success)
                    return NotFound(new { message });

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating medical history", error = ex.Message });
            }
        }

        /// <summary>
        /// Get patient appointments
        /// </summary>
        [HttpGet("{id}/appointments")]
        public async Task<ActionResult<PaginatedAppointmentsResponseDtoP>> GetPatientAppointments(
            string id,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _patientsRepository.GetPatientAppointmentsAsync(id, status, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving appointments", error = ex.Message });
            }
        }

        /// <summary>
        /// Get patient medical records
        /// </summary>
        [HttpGet("{id}/medical-records")]
        public async Task<ActionResult<PaginatedMedicalRecordsResponseDto>> GetPatientMedicalRecords(
            string id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _patientsRepository.GetPatientMedicalRecordsAsync(id, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving medical records", error = ex.Message });
            }
        }

        /// <summary>
        /// Get patient diagnoses
        /// </summary>
        [HttpGet("{id}/diagnoses")]
        public async Task<ActionResult<PaginatedDiagnosesResponseDto>> GetPatientDiagnoses(
            string id,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _patientsRepository.GetPatientDiagnosesAsync(id, status, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving diagnoses", error = ex.Message });
            }
        }

        /// <summary>
        /// Get patient treatments
        /// </summary>
        [HttpGet("{id}/treatments")]
        public async Task<ActionResult<PaginatedTreatmentsResponseDto>> GetPatientTreatments(
            string id,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _patientsRepository.GetPatientTreatmentsAsync(id, status, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving treatments", error = ex.Message });
            }
        }

        /// <summary>
        /// Get patient images
        /// </summary>
        [HttpGet("{id}/images")]
        public async Task<ActionResult<PaginatedImagesResponseDto>> GetPatientImages(
            string id,
            [FromQuery] string? imageType,
            [FromQuery] bool? isAnalyzedByAI,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _patientsRepository.GetPatientImagesAsync(id, imageType, isAnalyzedByAI, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving images", error = ex.Message });
            }
        }

        /// <summary>
        /// Get patient prescriptions
        /// </summary>
        [HttpGet("{id}/prescriptions")]
        public async Task<ActionResult<PaginatedPrescriptionsResponseDto>> GetPatientPrescriptions(
            string id,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _patientsRepository.GetPatientPrescriptionsAsync(id, status, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving prescriptions", error = ex.Message });
            }
        }

        /// <summary>
        /// Get patient documents
        /// </summary>
        [HttpGet("{id}/documents")]
        public async Task<ActionResult<PaginatedDocumentsResponseDto>> GetPatientDocuments(
            string id,
            [FromQuery] string? documentType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _patientsRepository.GetPatientDocumentsAsync(id, documentType, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving documents", error = ex.Message });
            }
        }

        /// <summary>
        /// Assign or change patient's assigned doctor
        /// </summary>
        [HttpPatch("{id}/assign-doctor")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult> AssignDoctorToPatient(string id, [FromBody] AssignDoctorDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var (success, message) = await _patientsRepository.AssignDoctorToPatientAsync(id, dto.DoctorId);

                if (!success)
                    return NotFound(new { message });

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while assigning doctor", error = ex.Message });
            }
        }
  
        /// <summary>
        /// Get patients statistics (Admin/Doctor only)
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<PatientStatisticsDto>> GetPatientsStatistics()
        {
            try
            {
                var result = await _patientsRepository.GetPatientsStatisticsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving statistics", error = ex.Message });
            }
        }
    }
}

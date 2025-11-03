using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmileCareAPI.DTOs;
using SmileCareAPI.Models;
using SmileCareAPI.Repositories.Interface;

namespace SmileCareAPI.Controllers
{
  
    [Route("api/medical-records")]
    [ApiController]
    [Authorize(Roles = "Doctor,Admin")]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly IMedicalRecordRepository _medicalRecordRepository;

        public MedicalRecordsController(IMedicalRecordRepository medicalRecordRepository)
        {
            _medicalRecordRepository = medicalRecordRepository;
        }

        // GET: api/medical-records
        [HttpGet]
        public async Task<ActionResult<MedGetAllRecordsResponseDto>> GetAllMedicalRecords(
            [FromQuery] string? patientId,
            [FromQuery] string? doctorId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                var (records, totalCount) = await _medicalRecordRepository.GetAllMedicalRecordsAsync(
                    patientId,
                    doctorId,
                    startDate,
                    endDate,
                    page,
                    pageSize);

                var response = new MedGetAllRecordsResponseDto
                {
                    Records = records.Select(r => new MedRecordSummaryDto
                    {
                        Id = r.Id,
                        VisitDate = r.VisitDate,
                        Patient = new MedPatientBasicDto
                        {
                            Id = r.Patient.UserId,
                            FirstName = r.Patient.User.FirstName,
                            LastName = r.Patient.User.LastName
                        },
                        Appointment = new MedAppointmentBasicDto
                        {
                            Id = r.Appointment.Id,
                            AppointmentDate = r.Appointment.AppointmentDate,
                            Type = r.Appointment.Type.ToString()
                        },
                        ChiefComplaint = r.ChiefComplaint,
                        ExaminationNotes = r.ExaminationNotes,
                        CreatedAt = r.CreatedAt
                    }).ToList(),
                    TotalCount = totalCount
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching medical records.", error = ex.Message });
            }
        }

        // GET: api/medical-records/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<MedGetRecordDetailsResponseDto>> GetMedicalRecordById(int id)
        {
            try
            {
                var record = await _medicalRecordRepository.GetMedicalRecordByIdAsync(id);

                if (record == null)
                {
                    return NotFound(new { message = "Medical record not found." });
                }

                var response = new MedGetRecordDetailsResponseDto
                {
                    Id = record.Id,
                    VisitDate = record.VisitDate,
                    ChiefComplaint = record.ChiefComplaint,
                    ExaminationNotes = record.ExaminationNotes,
                    BloodPressure = record.BloodPressure,
                    Temperature = record.Temperature,
                    Vitals = record.Vitals,
                    DoctorNotes = record.DoctorNotes,
                    Patient = new MedPatientDetailsDto
                    {
                        Id = record.Patient.UserId,
                        FirstName = record.Patient.User.FirstName,
                        LastName = record.Patient.User.LastName,
                        DateOfBirth = record.Patient.User.DateOfBirth,
                        Gender = record.Patient.User.Gender.ToString()
                    },
                    Appointment = new MedAppointmentBasicDto
                    {
                        Id = record.Appointment.Id,
                        AppointmentDate = record.Appointment.AppointmentDate,
                        Type = record.Appointment.Type.ToString()
                    },
                    Diagnoses = record.Diagnoses.Select(d => new MedDiagnosisDto
                    {
                        Id = d.Id,
                        DiagnosisName = d.DiagnosisName,
                        Severity = d.Severity.ToString(),
                        Description = d.Description
                    }).ToList(),
                    Treatments = record.Treatments.Select(t => new MedTreatmentDto
                    {
                        Id = t.Id,
                        TreatmentName = t.TreatmentName,
                        Status = t.Status.ToString(),
                        Description = t.Description
                    }).ToList(),
                    CreatedAt = record.CreatedAt,
                    UpdatedAt = record.UpdatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching medical record details.", error = ex.Message });
            }
        }

        // POST: api/medical-records
        [HttpPost]
        public async Task<ActionResult<MedRecordOperationResponseDto>> CreateMedicalRecord(
            [FromBody] MedCreateRecordRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate patient exists
                if (!await _medicalRecordRepository.PatientExistsAsync(request.PatientId))
                {
                    return NotFound(new { message = "Patient not found." });
                }

                // Validate appointment exists
                if (!await _medicalRecordRepository.AppointmentExistsAsync(request.AppointmentId))
                {
                    return NotFound(new { message = "Appointment not found." });
                }

                // Check if appointment already has a medical record
                if (await _medicalRecordRepository.IsAppointmentAlreadyLinkedAsync(request.AppointmentId))
                {
                    return BadRequest(new { message = "This appointment already has a medical record." });
                }

                var medicalRecord = new MedicalRecord
                {
                    PatientId = request.PatientId,
                    AppointmentId = request.AppointmentId,
                    VisitDate = request.VisitDate,
                    ChiefComplaint = request.ChiefComplaint,
                    ExaminationNotes = request.ExaminationNotes,
                    BloodPressure = request.BloodPressure,
                    Temperature = request.Temperature,
                    Vitals = request.Vitals,
                    DoctorNotes = request.DoctorNotes
                };

                var createdRecord = await _medicalRecordRepository.CreateMedicalRecordAsync(medicalRecord);

                return CreatedAtAction(
                    nameof(GetMedicalRecordById),
                    new { id = createdRecord.Id },
                    new MedRecordOperationResponseDto
                    {
                        Message = "Medical record created successfully.",
                        RecordId = createdRecord.Id
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating medical record.", error = ex.Message });
            }
        }

        // PUT: api/medical-records/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<MedRecordOperationResponseDto>> UpdateMedicalRecord(
            int id,
            [FromBody] MedUpdateRecordRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingRecord = await _medicalRecordRepository.GetMedicalRecordByIdAsync(id);

                if (existingRecord == null)
                {
                    return NotFound(new { message = "Medical record not found." });
                }

                // Update fields
                if (request.ChiefComplaint != null)
                    existingRecord.ChiefComplaint = request.ChiefComplaint;

                if (request.ExaminationNotes != null)
                    existingRecord.ExaminationNotes = request.ExaminationNotes;

                if (request.BloodPressure != null)
                    existingRecord.BloodPressure = request.BloodPressure;

                if (request.Temperature != null)
                    existingRecord.Temperature = request.Temperature;

                if (request.Vitals != null)
                    existingRecord.Vitals = request.Vitals;

                if (request.DoctorNotes != null)
                    existingRecord.DoctorNotes = request.DoctorNotes;

                await _medicalRecordRepository.UpdateMedicalRecordAsync(existingRecord);

                return Ok(new MedRecordOperationResponseDto
                {
                    Message = "Medical record updated successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating medical record.", error = ex.Message });
            }
        }

        // DELETE: api/medical-records/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<MedRecordOperationResponseDto>> DeleteMedicalRecord(int id)
        {
            try
            {
                if (!await _medicalRecordRepository.MedicalRecordExistsAsync(id))
                {
                    return NotFound(new { message = "Medical record not found." });
                }

                var result = await _medicalRecordRepository.DeleteMedicalRecordAsync(id);

                if (!result)
                {
                    return StatusCode(500, new { message = "Failed to delete medical record." });
                }

                return Ok(new MedRecordOperationResponseDto
                {
                    Message = "Medical record deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting medical record.", error = ex.Message });
            }
        }

        // GET: api/medical-records/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<MedGetPatientRecordsResponseDto>> GetMedicalRecordsByPatientId(
            string patientId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                // Validate patient exists
                if (!await _medicalRecordRepository.PatientExistsAsync(patientId))
                {
                    return NotFound(new { message = "Patient not found." });
                }

                var (records, totalCount) = await _medicalRecordRepository.GetMedicalRecordsByPatientIdAsync(
                    patientId,
                    page,
                    pageSize);

                var response = new MedGetPatientRecordsResponseDto
                {
                    Records = records.Select(r => new MedRecordSummaryDto
                    {
                        Id = r.Id,
                        VisitDate = r.VisitDate,
                        Patient = new MedPatientBasicDto
                        {
                            Id = r.Patient.UserId,
                            FirstName = r.Patient.User.FirstName,
                            LastName = r.Patient.User.LastName
                        },
                        Appointment = new MedAppointmentBasicDto
                        {
                            Id = r.Appointment.Id,
                            AppointmentDate = r.Appointment.AppointmentDate,
                            Type = r.Appointment.Type.ToString()
                        },
                        ChiefComplaint = r.ChiefComplaint,
                        ExaminationNotes = r.ExaminationNotes,
                        CreatedAt = r.CreatedAt
                    }).ToList(),
                    TotalCount = totalCount
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching patient medical records.", error = ex.Message });
            }
        }
    }
}

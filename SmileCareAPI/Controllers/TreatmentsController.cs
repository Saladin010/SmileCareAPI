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
    [Route("api/[controller]")]
    public class TreatmentsController : ControllerBase
    {
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly ILogger<TreatmentsController> _logger;

        public TreatmentsController(
            ITreatmentRepository treatmentRepository,
            ILogger<TreatmentsController> logger)
        {
            _treatmentRepository = treatmentRepository;
            _logger = logger;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User ID not found");
        }

        private string? GetCurrentUserRole()
        {
            return User.FindFirstValue(ClaimTypes.Role);
        }

        // 1. GET /api/treatments
        [HttpGet]
        public async Task<ActionResult<TRTreatmentListDto>> GetAllTreatments(
            [FromQuery] string? patientId,
            [FromQuery] string? doctorId,
            [FromQuery] TreatmentStatus? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Apply role-based filtering
                if (currentUserRole == "Patient")
                {
                    patientId = currentUserId;
                }
                else if (currentUserRole == "Doctor" && string.IsNullOrEmpty(doctorId))
                {
                    doctorId = currentUserId;
                }

                var (treatments, totalCount) = await _treatmentRepository.GetAllTreatmentsAsync(
                    patientId, doctorId, status, page, pageSize);

                var response = new TRTreatmentListDto
                {
                    Treatments = treatments.Select(t => new TRTreatmentListResponseDto
                    {
                        Id = t.Id,
                        TreatmentName = t.TreatmentName,
                        Status = t.Status,
                        StartDate = t.StartDate,
                        ExpectedCompletionDate = t.ExpectedCompletionDate,
                        TotalSessions = t.TotalSessions,
                        CompletedSessions = t.CompletedSessions,
                        Patient = new TRPatientBasicDto
                        {
                            Id = t.Patient.UserId,
                            FirstName = t.Patient.User.FirstName,
                            LastName = t.Patient.User.LastName
                        },
                        Doctor = new TRDoctorBasicDto
                        {
                            Id = t.Doctor.UserId,
                            FirstName = t.Doctor.User.FirstName,
                            LastName = t.Doctor.User.LastName
                        },
                        EstimatedCost = t.EstimatedCost,
                        PaidAmount = t.PaidAmount,
                        BalanceAmount = t.BalanceAmount,
                        PaymentStatus = t.PaymentStatus
                    }).ToList(),
                    TotalCount = totalCount
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting treatments list");
                return StatusCode(500, new { message = "An error occurred while retrieving treatments" });
            }
        }

        // 2. GET /api/treatments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TRTreatmentDetailsResponseDto>> GetTreatmentById(int id)
        {
            try
            {
                var treatment = await _treatmentRepository.GetTreatmentByIdAsync(id);
                if (treatment == null)
                    return NotFound(new { message = "Treatment not found" });

                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Authorization check
                if (currentUserRole == "Patient" && treatment.PatientId != currentUserId)
                    return Forbid();

                if (currentUserRole == "Doctor" && treatment.DoctorId != currentUserId)
                    return Forbid();

                var response = new TRTreatmentDetailsResponseDto
                {
                    Id = treatment.Id,
                    TreatmentName = treatment.TreatmentName,
                    Description = treatment.Description,
                    Status = treatment.Status,
                    ToothNumber = treatment.ToothNumber,
                    AffectedArea = treatment.AffectedArea,
                    StartDate = treatment.StartDate,
                    ExpectedCompletionDate = treatment.ExpectedCompletionDate,
                    ActualCompletionDate = treatment.ActualCompletionDate,
                    TotalSessions = treatment.TotalSessions,
                    CompletedSessions = treatment.CompletedSessions,
                    EstimatedCost = treatment.EstimatedCost,
                    PaidAmount = treatment.PaidAmount,
                    BalanceAmount = treatment.BalanceAmount,
                    PaymentStatus = treatment.PaymentStatus,
                    ProgressNotes = treatment.ProgressNotes,
                    Complications = treatment.Complications,
                    PatientFeedback = treatment.PatientFeedback,
                    BeforeImageUrl = treatment.BeforeImageUrl,
                    AfterImageUrl = treatment.AfterImageUrl,
                    Patient = new TRPatientInfoDto
                    {
                        Id = treatment.Patient.UserId,
                        FirstName = treatment.Patient.User.FirstName,
                        LastName = treatment.Patient.User.LastName,
                        PhoneNumber = treatment.Patient.User.PhoneNumber ?? ""
                    },
                    Doctor = new TRDoctorInfoDto
                    {
                        Id = treatment.Doctor.UserId,
                        FirstName = treatment.Doctor.User.FirstName,
                        LastName = treatment.Doctor.User.LastName,
                        Specialization = treatment.Doctor.Specialization
                    },
                    Diagnosis = treatment.Diagnosis != null ? new TRDiagnosisBasicDto
                    {
                        Id = treatment.Diagnosis.Id,
                        DiagnosisName = treatment.Diagnosis.DiagnosisName,
                        Severity = treatment.Diagnosis.Severity
                    } : null,
                    MedicalRecord = treatment.MedicalRecord != null ? new TRMedicalRecordBasicDto
                    {
                        Id = treatment.MedicalRecord.Id,
                        VisitDate = treatment.MedicalRecord.VisitDate
                    } : null,
                    Sessions = treatment.Sessions.Select(s => new TRTreatmentSessionDto
                    {
                        Id = s.Id,
                        SessionNumber = s.SessionNumber,
                        ScheduledDate = s.ScheduledDate,
                        CompletedDate = s.CompletedDate,
                        IsCompleted = s.IsCompleted,
                        Notes = s.Notes,
                        SessionCost = s.SessionCost
                    }).ToList(),
                    Prescriptions = treatment.Prescriptions.Select(p => new TRPrescriptionBasicDto
                    {
                        Id = p.Id,
                        IssuedDate = p.IssuedDate,
                        Status = p.Status
                    }).ToList(),
                    Images = treatment.Images.Select(i => new TRImageBasicDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        UploadedAt = i.UploadedAt
                    }).ToList(),
                    CreatedAt = treatment.CreatedAt,
                    UpdatedAt = treatment.UpdatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting treatment details for ID: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving treatment details" });
            }
        }

        // 3. POST /api/treatments
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult> CreateTreatment([FromBody] TRCreateTreatmentRequestDto request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var treatment = new Treatment
                {
                    PatientId = request.PatientId,
                    DoctorId = currentUserId,
                    DiagnosisId = request.DiagnosisId,
                    TreatmentName = request.TreatmentName,
                    Description = request.Description,
                    ToothNumber = request.ToothNumber,
                    AffectedArea = request.AffectedArea,
                    StartDate = request.StartDate,
                    ExpectedCompletionDate = request.ExpectedCompletionDate,
                    TotalSessions = request.TotalSessions,
                    EstimatedCost = request.EstimatedCost,
                    ProgressNotes = request.ProgressNotes,
                    Status = TreatmentStatus.Planned,
                    PaymentStatus = PaymentStatus.Pending,
                    BalanceAmount = request.EstimatedCost
                };

                var created = await _treatmentRepository.CreateTreatmentAsync(treatment);

                return CreatedAtAction(
                    nameof(GetTreatmentById),
                    new { id = created.Id },
                    new { message = "Treatment plan created successfully", treatmentId = created.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating treatment");
                return StatusCode(500, new { message = "An error occurred while creating treatment" });
            }
        }

        // 4. PUT /api/treatments/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult> UpdateTreatment(int id, [FromBody] TRUpdateTreatmentRequestDto request)
        {
            try
            {
                var treatment = await _treatmentRepository.GetTreatmentByIdAsync(id);
                if (treatment == null)
                    return NotFound(new { message = "Treatment not found" });

                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Only assigned doctor or admin can update
                if (currentUserRole == "Doctor" && treatment.DoctorId != currentUserId)
                    return Forbid();

                if (request.TreatmentName != null)
                    treatment.TreatmentName = request.TreatmentName;

                if (request.Description != null)
                    treatment.Description = request.Description;

                if (request.Status.HasValue)
                    treatment.Status = request.Status.Value;

                if (request.ExpectedCompletionDate.HasValue)
                    treatment.ExpectedCompletionDate = request.ExpectedCompletionDate;

                if (request.ProgressNotes != null)
                    treatment.ProgressNotes = request.ProgressNotes;

                if (request.Complications != null)
                    treatment.Complications = request.Complications;

                if (request.PatientFeedback != null)
                    treatment.PatientFeedback = request.PatientFeedback;

                var updated = await _treatmentRepository.UpdateTreatmentAsync(treatment);
                if (!updated)
                    return BadRequest(new { message = "Failed to update treatment" });

                return Ok(new { message = "Treatment updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating treatment ID: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while updating treatment" });
            }
        }

        // 5. DELETE /api/treatments/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult> DeleteTreatment(int id)
        {
            try
            {
                var treatment = await _treatmentRepository.GetTreatmentByIdAsync(id);
                if (treatment == null)
                    return NotFound(new { message = "Treatment not found" });

                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Only assigned doctor or admin can delete
                if (currentUserRole == "Doctor" && treatment.DoctorId != currentUserId)
                    return Forbid();

                var deleted = await _treatmentRepository.DeleteTreatmentAsync(id);
                if (!deleted)
                    return BadRequest(new { message = "Failed to delete treatment" });

                return Ok(new { message = "Treatment deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting treatment ID: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while deleting treatment" });
            }
        }

        // 6. PATCH /api/treatments/{id}/status
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult> UpdateTreatmentStatus(
            int id,
            [FromBody] TRUpdateTreatmentStatusRequestDto request)
        {
            try
            {
                var exists = await _treatmentRepository.TreatmentExistsAsync(id);
                if (!exists)
                    return NotFound(new { message = "Treatment not found" });

                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                if (currentUserRole == "Doctor" && !await _treatmentRepository.IsDoctorAssignedAsync(id, currentUserId))
                    return Forbid();

                var updated = await _treatmentRepository.UpdateTreatmentStatusAsync(id, request.Status);
                if (!updated)
                    return BadRequest(new { message = "Failed to update treatment status" });

                return Ok(new { message = "Treatment status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating treatment status for ID: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while updating treatment status" });
            }
        }

        // 7. POST /api/treatments/{id}/sessions
        [HttpPost("{id}/sessions")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult> CreateTreatmentSession(
            int id,
            [FromBody] TRCreateTreatmentSessionRequestDto request)
        {
            try
            {
                var exists = await _treatmentRepository.TreatmentExistsAsync(id);
                if (!exists)
                    return NotFound(new { message = "Treatment not found" });

                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                if (currentUserRole == "Doctor" && !await _treatmentRepository.IsDoctorAssignedAsync(id, currentUserId))
                    return Forbid();

                var session = new TreatmentSession
                {
                    TreatmentId = id,
                    SessionNumber = request.SessionNumber,
                    ScheduledDate = request.ScheduledDate,
                    Notes = request.Notes,
                    SessionCost = request.SessionCost
                };

                var created = await _treatmentRepository.CreateTreatmentSessionAsync(session);

                return Ok(new { message = "Treatment session created successfully", sessionId = created.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating treatment session for treatment ID: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while creating treatment session" });
            }
        }

        // 8. PATCH /api/treatments/{id}/sessions/{sessionId}/complete
        [HttpPatch("{id}/sessions/{sessionId}/complete")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult> CompleteTreatmentSession(
            int id,
            int sessionId,
            [FromBody] TRCompleteTreatmentSessionRequestDto request)
        {
            try
            {
                var exists = await _treatmentRepository.TreatmentExistsAsync(id);
                if (!exists)
                    return NotFound(new { message = "Treatment not found" });

                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                if (currentUserRole == "Doctor" && !await _treatmentRepository.IsDoctorAssignedAsync(id, currentUserId))
                    return Forbid();

                var completed = await _treatmentRepository.CompleteTreatmentSessionAsync(
                    id,
                    sessionId,
                    request.CompletedDate,
                    request.Notes);

                if (!completed)
                    return NotFound(new { message = "Treatment session not found" });

                return Ok(new { message = "Treatment session completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing treatment session {SessionId} for treatment ID: {Id}", sessionId, id);
                return StatusCode(500, new { message = "An error occurred while completing treatment session" });
            }
        }

        //// 9. POST /api/treatments/{id}/upload-images
        //[HttpPost("{id}/upload-images")]
        //[Authorize(Roles = "Admin,Doctor")]
        //public async Task<ActionResult> UploadTreatmentImages(
        //    int id,
        //    [FromForm] IFormFile? beforeImage,
        //    [FromForm] IFormFile? afterImage)
        //{
        //    try
        //    {
        //        var exists = await _treatmentRepository.TreatmentExistsAsync(id);
        //        if (!exists)
        //            return NotFound(new { message = "Treatment not found" });

        //        var currentUserId = GetCurrentUserId();
        //        var currentUserRole = GetCurrentUserRole();

        //        if (currentUserRole == "Doctor" && !await _treatmentRepository.IsDoctorAssignedAsync(id, currentUserId))
        //            return Forbid();

        //        string? beforeImageUrl = null;
        //        string? afterImageUrl = null;

        //        // TODO: Implement actual file upload logic to cloud storage
        //        // For now, return placeholder URLs
        //        if (beforeImage != null)
        //        {
        //            // beforeImageUrl = await UploadFileAsync(beforeImage, "treatments/before");
        //            beforeImageUrl = $"/uploads/treatments/{id}/before_{Guid.NewGuid()}{Path.GetExtension(beforeImage.FileName)}";
        //        }

        //        if (afterImage != null)
        //        {
        //            // afterImageUrl = await UploadFileAsync(afterImage, "treatments/after");
        //            afterImageUrl = $"/uploads/treatments/{id}/after_{Guid.NewGuid()}{Path.GetExtension(afterImage.FileName)}";
        //        }

        //        var updated = await _treatmentRepository.UpdateTreatmentImagesAsync(id, beforeImageUrl, afterImageUrl);
        //        if (!updated)
        //            return BadRequest(new { message = "Failed to update treatment images" });

        //        return Ok(new
        //        {
        //            message = "Treatment images uploaded successfully",
        //            beforeImageUrl,
        //            afterImageUrl
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error uploading treatment images for ID: {Id}", id);
        //        return StatusCode(500, new { message = "An error occurred while uploading treatment images" });
        //    }
        //}
        [HttpPost("{id}/upload-images")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult> UploadTreatmentImages(int id, [FromForm] TreatmentImagesUploadDto model)
        {
            try
            {
                var exists = await _treatmentRepository.TreatmentExistsAsync(id);
                if (!exists)
                    return NotFound(new { message = "Treatment not found" });

                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                if (currentUserRole == "Doctor" && !await _treatmentRepository.IsDoctorAssignedAsync(id, currentUserId))
                    return Forbid();

                string? beforeImageUrl = null;
                string? afterImageUrl = null;

                if (model.BeforeImage != null)
                {
                    beforeImageUrl = $"/uploads/treatments/{id}/before_{Guid.NewGuid()}{Path.GetExtension(model.BeforeImage.FileName)}";
                }

                if (model.AfterImage != null)
                {
                    afterImageUrl = $"/uploads/treatments/{id}/after_{Guid.NewGuid()}{Path.GetExtension(model.AfterImage.FileName)}";
                }

                var updated = await _treatmentRepository.UpdateTreatmentImagesAsync(id, beforeImageUrl, afterImageUrl);
                if (!updated)
                    return BadRequest(new { message = "Failed to update treatment images" });

                return Ok(new
                {
                    message = "Treatment images uploaded successfully",
                    beforeImageUrl,
                    afterImageUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading treatment images for ID: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while uploading treatment images" });
            }
        }

        // 10. PATCH /api/treatments/{id}/payment
        [HttpPatch("{id}/payment")]
        [Authorize(Roles = "Doctor,Receptionist")]
        public async Task<ActionResult> UpdateTreatmentPayment(
            int id,
            [FromBody] TRUpdatePaymentRequestDto request)
        {
            try
            {
                var exists = await _treatmentRepository.TreatmentExistsAsync(id);
                if (!exists)
                    return NotFound(new { message = "Treatment not found" });

                var updated = await _treatmentRepository.UpdateTreatmentPaymentAsync(
                    id,
                    request.PaidAmount,
                    request.PaymentStatus);

                if (!updated)
                    return BadRequest(new { message = "Failed to update payment information" });

                var treatment = await _treatmentRepository.GetTreatmentByIdAsync(id);

                return Ok(new
                {
                    message = "Payment updated successfully",
                    balanceAmount = treatment?.BalanceAmount ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment for treatment ID: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while updating payment" });
            }
        }

        // 11. GET /api/treatments/statistics
        [HttpGet("statistics")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<TRTreatmentStatisticsDto>> GetTreatmentStatistics(
            [FromQuery] string? doctorId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Doctor can only see their own statistics
                if (currentUserRole == "Doctor")
                {
                    doctorId = currentUserId;
                }

                var statistics = await _treatmentRepository.GetTreatmentStatisticsAsync(
                    doctorId,
                    startDate,
                    endDate);

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting treatment statistics");
                return StatusCode(500, new { message = "An error occurred while retrieving statistics" });
            }
        }
    }
}

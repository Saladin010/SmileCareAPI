using Microsoft.EntityFrameworkCore;
using SmileCareAPI.Data;
using SmileCareAPI.DTOs;
using SmileCareAPI.Models;
using SmileCareAPI.Repositories.Interface;

namespace SmileCareAPI.Repositories.Implementation
{
    public class DIAGDiagnosisRepository : IDIAGDiagnosisRepository
    {
        private readonly ApplicationDbContext _context;

        public DIAGDiagnosisRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DIAGDiagnosisListResponseDto> GetAllDiagnosesAsync(
            string? patientId = null,
            string? doctorId = null,
            DiagnosisStatus? status = null,
            bool? isAIAssisted = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Diagnoses
                .Include(d => d.Patient)
                    .ThenInclude(p => p.User)
                .Include(d => d.Doctor)
                    .ThenInclude(doc => doc.User)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(patientId))
                query = query.Where(d => d.PatientId == patientId);

            if (!string.IsNullOrEmpty(doctorId))
                query = query.Where(d => d.DoctorId == doctorId);

            if (status.HasValue)
                query = query.Where(d => d.Status == status.Value);

            if (isAIAssisted.HasValue)
                query = query.Where(d => d.IsAIAssisted == isAIAssisted.Value);

            var totalCount = await query.CountAsync();

            var diagnoses = await query
                .OrderByDescending(d => d.DiagnosisDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DIAGDiagnosisListDto
                {
                    Id = d.Id,
                    DiagnosisName = d.DiagnosisName,
                    Severity = d.Severity,
                    Status = d.Status,
                    DiagnosisDate = d.DiagnosisDate,
                    Patient = new DIAGPatientBasicDto
                    {
                        Id = d.Patient.UserId,
                        FirstName = d.Patient.User.FirstName,
                        LastName = d.Patient.User.LastName
                    },
                    Doctor = new DIAGDoctorBasicDto
                    {
                        Id = d.Doctor.UserId,
                        FirstName = d.Doctor.User.FirstName,
                        LastName = d.Doctor.User.LastName
                    },
                    IsAIAssisted = d.IsAIAssisted,
                    AffectedArea = d.AffectedArea,
                    ToothNumber = d.ToothNumber
                })
                .ToListAsync();

            return new DIAGDiagnosisListResponseDto
            {
                Diagnoses = diagnoses,
                TotalCount = totalCount
            };
        }

        public async Task<DIAGDiagnosisDetailDto?> GetDiagnosisByIdAsync(int id)
        {
            var diagnosis = await _context.Diagnoses
                .Include(d => d.Patient)
                    .ThenInclude(p => p.User)
                .Include(d => d.Doctor)
                    .ThenInclude(doc => doc.User)
                .Include(d => d.Doctor.User)
                .Include(d => d.MedicalRecord)
                .Include(d => d.AIResult)
                .Include(d => d.Treatments)
                .Include(d => d.Images)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (diagnosis == null)
                return null;

            return new DIAGDiagnosisDetailDto
            {
                Id = diagnosis.Id,
                DiagnosisName = diagnosis.DiagnosisName,
                Description = diagnosis.Description,
                Severity = diagnosis.Severity,
                Status = diagnosis.Status,
                AffectedArea = diagnosis.AffectedArea,
                ToothNumber = diagnosis.ToothNumber,
                Notes = diagnosis.Notes,
                DiagnosisDate = diagnosis.DiagnosisDate,
                ResolvedDate = diagnosis.ResolvedDate,
                UpdatedAt = diagnosis.UpdatedAt,
                Patient = new DIAGPatientDetailDto
                {
                    Id = diagnosis.Patient.UserId,
                    FirstName = diagnosis.Patient.User.FirstName,
                    LastName = diagnosis.Patient.User.LastName,
                    DateOfBirth = diagnosis.Patient.User.DateOfBirth,
                    Gender = diagnosis.Patient.User.Gender
                },
                Doctor = new DIAGDoctorDetailDto
                {
                    Id = diagnosis.Doctor.UserId,
                    FirstName = diagnosis.Doctor.User.FirstName,
                    LastName = diagnosis.Doctor.User.LastName,
                    Specialization = diagnosis.Doctor.Specialization
                },
                MedicalRecord = diagnosis.MedicalRecord != null ? new DIAGMedicalRecordBasicDto
                {
                    Id = diagnosis.MedicalRecord.Id,
                    VisitDate = diagnosis.MedicalRecord.VisitDate,
                    ChiefComplaint = diagnosis.MedicalRecord.ChiefComplaint
                } : null,
                AIResult = diagnosis.AIResult != null ? new DIAGAIResultDto
                {
                    Id = diagnosis.AIResult.Id,
                    PrimaryResult = diagnosis.AIResult.PrimaryResult,
                    ConfidenceScore = diagnosis.AIResult.ConfidenceScore,
                    HealthyProbability = diagnosis.AIResult.HealthyProbability,
                    CavityProbability = diagnosis.AIResult.CavityProbability,
                    GumDiseaseProbability = diagnosis.AIResult.GumDiseaseProbability,
                    HeatmapImageUrl = diagnosis.AIResult.HeatmapImageUrl,
                    IsConfirmedByDoctor = diagnosis.AIResult.IsConfirmedByDoctor,
                    DoctorAgreement = diagnosis.AIResult.DoctorAgreement
                } : null,
                Treatments = diagnosis.Treatments.Select(t => new DIAGTreatmentBasicDto
                {
                    Id = t.Id,
                    TreatmentName = t.TreatmentName,
                    Status = t.Status
                }).ToList(),
                Images = diagnosis.Images.Select(img => new DIAGImageBasicDto
                {
                    Id = img.Id,
                    ImageUrl = img.ImageUrl,
                    ImageType = img.ImageType,
                    UploadedAt = img.UploadedAt
                }).ToList()
            };
        }

        public async Task<DIAGCreateResponseDto> CreateDiagnosisAsync(DIAGCreateDiagnosisDto dto, string doctorId)
        {
            // Validate patient exists
            var patientExists = await _context.Patients.AnyAsync(p => p.UserId == dto.PatientId);
            if (!patientExists)
                throw new ArgumentException("Patient not found");

            // Validate medical record if provided
            if (dto.MedicalRecordId.HasValue)
            {
                var recordExists = await _context.MedicalRecords
                    .AnyAsync(mr => mr.Id == dto.MedicalRecordId.Value && mr.PatientId == dto.PatientId);
                if (!recordExists)
                    throw new ArgumentException("Medical record not found or does not belong to patient");
            }

            // Validate AI result if provided
            if (dto.AIResultId.HasValue)
            {
                var aiResultExists = await _context.AIResults
                    .AnyAsync(ar => ar.Id == dto.AIResultId.Value && ar.PatientId == dto.PatientId);
                if (!aiResultExists)
                    throw new ArgumentException("AI result not found or does not belong to patient");
            }

            var diagnosis = new Models.Diagnosis
            {
                PatientId = dto.PatientId,
                DoctorId = doctorId,
                DiagnosisName = dto.DiagnosisName,
                Description = dto.Description,
                Severity = dto.Severity,
                AffectedArea = dto.AffectedArea,
                ToothNumber = dto.ToothNumber,
                MedicalRecordId = dto.MedicalRecordId,
                Notes = dto.Notes,
                IsAIAssisted = dto.IsAIAssisted,
                AIResultId = dto.AIResultId,
                Status = DiagnosisStatus.Active,
                DiagnosisDate = DateTime.UtcNow
            };

            _context.Diagnoses.Add(diagnosis);
            await _context.SaveChangesAsync();

            return new DIAGCreateResponseDto
            {
                Message = "Diagnosis created successfully",
                DiagnosisId = diagnosis.Id
            };
        }

        public async Task<DIAGMessageResponseDto> UpdateDiagnosisAsync(int id, DIAGUpdateDiagnosisDto dto, string userId)
        {
            var diagnosis = await _context.Diagnoses.FindAsync(id);
            if (diagnosis == null)
                throw new KeyNotFoundException("Diagnosis not found");

            // Update only provided fields
            if (!string.IsNullOrEmpty(dto.DiagnosisName))
                diagnosis.DiagnosisName = dto.DiagnosisName;

            if (dto.Description != null)
                diagnosis.Description = dto.Description;

            if (dto.Severity.HasValue)
                diagnosis.Severity = dto.Severity.Value;

            if (dto.Status.HasValue)
            {
                diagnosis.Status = dto.Status.Value;
                if (dto.Status.Value == DiagnosisStatus.Resolved)
                    diagnosis.ResolvedDate = DateTime.UtcNow;
            }

            if (dto.Notes != null)
                diagnosis.Notes = dto.Notes;

            diagnosis.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new DIAGMessageResponseDto { Message = "Diagnosis updated successfully" };
        }

        public async Task<DIAGMessageResponseDto> DeleteDiagnosisAsync(int id, string userId)
        {
            var diagnosis = await _context.Diagnoses
                .Include(d => d.Treatments)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (diagnosis == null)
                throw new KeyNotFoundException("Diagnosis not found");

            // Check if diagnosis has associated treatments
            if (diagnosis.Treatments.Any())
                throw new InvalidOperationException("Cannot delete diagnosis with associated treatments");

            _context.Diagnoses.Remove(diagnosis);
            await _context.SaveChangesAsync();

            return new DIAGMessageResponseDto { Message = "Diagnosis deleted successfully" };
        }

        public async Task<DIAGMessageResponseDto> UpdateDiagnosisStatusAsync(int id, DIAGUpdateStatusDto dto, string userId)
        {
            var diagnosis = await _context.Diagnoses.FindAsync(id);
            if (diagnosis == null)
                throw new KeyNotFoundException("Diagnosis not found");

            diagnosis.Status = dto.Status;
            diagnosis.UpdatedAt = DateTime.UtcNow;

            if (dto.Status == DiagnosisStatus.Resolved)
                diagnosis.ResolvedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new DIAGMessageResponseDto { Message = "Diagnosis status updated successfully" };
        }

        public async Task<DIAGStatisticsDto> GetDiagnosisStatisticsAsync(
            string? doctorId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _context.Diagnoses.AsQueryable();

            if (!string.IsNullOrEmpty(doctorId))
                query = query.Where(d => d.DoctorId == doctorId);

            if (startDate.HasValue)
                query = query.Where(d => d.DiagnosisDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(d => d.DiagnosisDate <= endDate.Value);

            var totalDiagnoses = await query.CountAsync();
            var activeConditions = await query.CountAsync(d => d.Status == DiagnosisStatus.Active);
            var resolvedConditions = await query.CountAsync(d => d.Status == DiagnosisStatus.Resolved);

            var diagnosisDistribution = await query
                .GroupBy(d => d.DiagnosisName)
                .Select(g => new DIAGDiagnosisDistributionDto
                {
                    DiagnosisName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var mildCount = await query.CountAsync(d => d.Severity == SeverityLevel.Mild);
            var moderateCount = await query.CountAsync(d => d.Severity == SeverityLevel.Moderate);
            var severeCount = await query.CountAsync(d => d.Severity == SeverityLevel.Severe);

            var aiAssistedCount = await query.CountAsync(d => d.IsAIAssisted);
            var manualCount = await query.CountAsync(d => !d.IsAIAssisted);

            return new DIAGStatisticsDto
            {
                TotalDiagnoses = totalDiagnoses,
                ActiveConditions = activeConditions,
                ResolvedConditions = resolvedConditions,
                DiagnosisDistribution = diagnosisDistribution,
                SeverityDistribution = new DIAGSeverityDistributionDto
                {
                    Mild = mildCount,
                    Moderate = moderateCount,
                    Severe = severeCount
                },
                AIAssistedCount = aiAssistedCount,
                ManualCount = manualCount
            };
        }

        public async Task<bool> DiagnosisExistsAsync(int id)
        {
            return await _context.Diagnoses.AnyAsync(d => d.Id == id);
        }

        public async Task<bool> CanUserAccessDiagnosisAsync(int diagnosisId, string userId, UserRole userRole)
        {
            var diagnosis = await _context.Diagnoses.FindAsync(diagnosisId);
            if (diagnosis == null)
                return false;

            return userRole switch
            {
                UserRole.Admin => true,
                UserRole.Doctor => diagnosis.DoctorId == userId,
                UserRole.Patient => diagnosis.PatientId == userId,
                _ => false
            };
        }
    }
}

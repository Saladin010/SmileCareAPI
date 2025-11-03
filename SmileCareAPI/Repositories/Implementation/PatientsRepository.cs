using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmileCareAPI.Data;
using SmileCareAPI.DTOs;
using SmileCareAPI.Models;
using SmileCareAPI.Repositories.Interface;

namespace SmileCareAPI.Repositories.Implementation
{
    public class PatientsRepository : IPatientsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public PatientsRepository(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<PaginatedPatientsResponseDtoP> GetAllPatientsAsync(PatientFilterDto filter)
        {
            var query = _context.Patients
                .Include(p => p.User)
                .Include(p => p.AssignedDoctor)
                    .ThenInclude(d => d.User)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(p =>
                    p.User.FirstName.Contains(filter.Search) ||
                    p.User.LastName.Contains(filter.Search) ||
                    p.User.Email.Contains(filter.Search) ||
                    p.User.PhoneNumber.Contains(filter.Search));
            }

            if (!string.IsNullOrWhiteSpace(filter.Gender))
            {
                if (Enum.TryParse<Gender>(filter.Gender, true, out var gender))
                {
                    query = query.Where(p => p.User.Gender == gender);
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                bool isActive = filter.Status.ToLower() == "active";
                query = query.Where(p => p.User.IsActive == isActive);
            }

            if (!string.IsNullOrWhiteSpace(filter.AssignedDoctorId))
            {
                query = query.Where(p => p.AssignedDoctorId == filter.AssignedDoctorId);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            var patients = await query
                .OrderByDescending(p => p.User.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new PatientListItemDto
                {
                    Id = p.UserId,
                    FirstName = p.User.FirstName,
                    LastName = p.User.LastName,
                    Email = p.User.Email,
                    PhoneNumber = p.User.PhoneNumber,
                    DateOfBirth = p.User.DateOfBirth,
                    Gender = p.User.Gender.ToString(),
                    City = p.User.City,
                    ProfilePicture = p.User.ProfilePicture,
                    TotalVisits = p.TotalVisits,
                    LastVisitDate = p.LastVisitDate,
                    IsActive = p.User.IsActive,
                    AssignedDoctor = p.AssignedDoctor != null ? new AssignedDoctorDto
                    {
                        Id = p.AssignedDoctor.UserId,
                        FirstName = p.AssignedDoctor.User.FirstName,
                        LastName = p.AssignedDoctor.User.LastName
                    } : null
                })
                .ToListAsync();

            return new PaginatedPatientsResponseDtoP
            {
                Patients = patients,
                TotalCount = totalCount,
                CurrentPage = filter.Page,
                TotalPages = totalPages
            };
        }

        public async Task<PatientDetailsResponseDto> GetPatientByIdAsync(string patientId)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .Include(p => p.AssignedDoctor)
                    .ThenInclude(d => d.User)
                .Include(p => p.MedicalHistory)
                .FirstOrDefaultAsync(p => p.UserId == patientId);

            if (patient == null)
                return null;

            var totalAppointments = await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .CountAsync();

            var upcomingAppointments = await _context.Appointments
                .Where(a => a.PatientId == patientId &&
                           a.AppointmentDate >= DateTime.UtcNow &&
                           a.Status != AppointmentStatus.Cancelled)
                .CountAsync();

            var totalDiagnoses = await _context.Diagnoses
                .Where(d => d.PatientId == patientId)
                .CountAsync();

            var totalTreatments = await _context.Treatments
                .Where(t => t.PatientId == patientId)
                .CountAsync();

            var totalImages = await _context.PatientImages
                .Where(i => i.PatientId == patientId)
                .CountAsync();

            return new PatientDetailsResponseDto
            {
                User = new UserInfoDtoP
                {
                    Id = patient.User.Id,
                    FirstName = patient.User.FirstName,
                    LastName = patient.User.LastName,
                    Email = patient.User.Email,
                    PhoneNumber = patient.User.PhoneNumber,
                    Gender = patient.User.Gender.ToString(),
                    DateOfBirth = patient.User.DateOfBirth,
                    City = patient.User.City,
                    DetailedAddress = patient.User.DetailedAddress,
                    ProfilePicture = patient.User.ProfilePicture
                },
                Patient = new PatientInfoDtoP
                {
                    EmergencyContactName = patient.EmergencyContactName,
                    EmergencyContactRelationship = patient.EmergencyContactRelationship,
                    EmergencyContactPhone = patient.EmergencyContactPhone,
                    EmergencyContactAlternativePhone = patient.EmergencyContactAlternativePhone,
                    AssignedDoctor = patient.AssignedDoctor != null ? new AssignedDoctorDetailedDto
                    {
                        Id = patient.AssignedDoctor.UserId,
                        FirstName = patient.AssignedDoctor.User.FirstName,
                        LastName = patient.AssignedDoctor.User.LastName,
                        Specialization = patient.AssignedDoctor.Specialization
                    } : null,
                    BloodType = patient.BloodType,
                    TotalVisits = patient.TotalVisits,
                    LastVisitDate = patient.LastVisitDate
                },
                MedicalHistory = patient.MedicalHistory != null ? new MedicalHistoryDto
                {
                    ChronicDiseases = patient.MedicalHistory.ChronicDiseases,
                    Allergies = patient.MedicalHistory.Allergies,
                    CurrentMedications = patient.MedicalHistory.CurrentMedications,
                    PreviousDentalSurgeries = patient.MedicalHistory.PreviousDentalSurgeries,
                    AdditionalNotes = patient.MedicalHistory.AdditionalNotes,
                    CreatedAt = patient.MedicalHistory.CreatedAt,
                    UpdatedAt = patient.MedicalHistory.UpdatedAt
                } : null,
                Statistics = new PatientStatisticsDetailsDto
                {
                    TotalAppointments = totalAppointments,
                    UpcomingAppointments = upcomingAppointments,
                    TotalDiagnoses = totalDiagnoses,
                    TotalTreatments = totalTreatments,
                    TotalImages = totalImages
                }
            };
        }

        public async Task<(bool Success, string Message, string PatientId)> CreatePatientAsync(CreatePatientDto dto)
        {
            // Check if email or username already exists
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return (false, "Email already exists", null);

            if (await _userManager.FindByNameAsync(dto.Username) != null)
                return (false, "Username already exists", null);

            // Parse gender
            if (!Enum.TryParse<Gender>(dto.Gender, true, out var gender))
                return (false, "Invalid gender value", null);

            // Verify doctor exists if assigned
            if (!string.IsNullOrWhiteSpace(dto.AssignedDoctorId))
            {
                var doctorExists = await _context.Doctors.AnyAsync(d => d.UserId == dto.AssignedDoctorId);
                if (!doctorExists)
                    return (false, "Assigned doctor not found", null);
            }

            var user = new User
            {
                UserName = dto.Username,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                Gender = gender,
                DateOfBirth = dto.DateOfBirth,
                City = dto.City,
                DetailedAddress = dto.DetailedAddress,
                Role = UserRole.Patient,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);

            var patient = new Patient
            {
                UserId = user.Id,
                EmergencyContactName = dto.EmergencyContactName,
                EmergencyContactPhone = dto.EmergencyContactPhone,
                AssignedDoctorId = dto.AssignedDoctorId
            };
            await _userManager.AddToRoleAsync(user, user.Role.ToString());

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            // Update doctor's total patients count
            if (!string.IsNullOrWhiteSpace(dto.AssignedDoctorId))
            {
                var doctor = await _context.Doctors.FindAsync(dto.AssignedDoctorId);
                if (doctor != null)
                {
                    doctor.TotalPatients++;
                    await _context.SaveChangesAsync();
                }
            }

            return (true, "Patient created successfully", user.Id);
        }

        public async Task<(bool Success, string Message)> UpdatePatientAsync(string patientId, UpdatePatientDto dto)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == patientId);

            if (patient == null)
                return (false, "Patient not found");

            patient.User.FirstName = dto.FirstName;
            patient.User.LastName = dto.LastName;
            patient.User.PhoneNumber = dto.PhoneNumber;
            patient.User.City = dto.City;
            patient.User.DetailedAddress = dto.DetailedAddress;

            patient.EmergencyContactName = dto.EmergencyContactName;
            patient.EmergencyContactRelationship = dto.EmergencyContactRelationship;
            patient.EmergencyContactPhone = dto.EmergencyContactPhone;
            patient.EmergencyContactAlternativePhone = dto.EmergencyContactAlternativePhone;
            patient.BloodType = dto.BloodType;

            await _context.SaveChangesAsync();
            return (true, "Patient updated successfully");
        }

        public async Task<(bool Success, string Message)> DeletePatientAsync(string patientId)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == patientId);

            if (patient == null)
                return (false, "Patient not found");

            // Check if patient has any appointments
            var hasAppointments = await _context.Appointments
                .AnyAsync(a => a.PatientId == patientId);

            if (hasAppointments)
            {
                // Soft delete by deactivating
                patient.User.IsActive = false;
                await _context.SaveChangesAsync();
                return (true, "Patient deactivated successfully");
            }

            // Hard delete if no appointments
            _context.Patients.Remove(patient);
            await _userManager.DeleteAsync(patient.User);
            return (true, "Patient deleted successfully");
        }

        public async Task<MedicalHistoryDto> GetPatientMedicalHistoryAsync(string patientId)
        {
            var medicalHistory = await _context.MedicalHistories
                .FirstOrDefaultAsync(m => m.PatientId == patientId);

            if (medicalHistory == null)
                return null;

            return new MedicalHistoryDto
            {
                ChronicDiseases = medicalHistory.ChronicDiseases,
                Allergies = medicalHistory.Allergies,
                CurrentMedications = medicalHistory.CurrentMedications,
                PreviousDentalSurgeries = medicalHistory.PreviousDentalSurgeries,
                AdditionalNotes = medicalHistory.AdditionalNotes,
                CreatedAt = medicalHistory.CreatedAt,
                UpdatedAt = medicalHistory.UpdatedAt
            };
        }

        public async Task<(bool Success, string Message)> UpdatePatientMedicalHistoryAsync(string patientId, UpdateMedicalHistoryDto dto)
        {
            var patientExists = await _context.Patients.AnyAsync(p => p.UserId == patientId);
            if (!patientExists)
                return (false, "Patient not found");

            var medicalHistory = await _context.MedicalHistories
                .FirstOrDefaultAsync(m => m.PatientId == patientId);

            if (medicalHistory == null)
            {
                medicalHistory = new MedicalHistory
                {
                    PatientId = patientId,
                    ChronicDiseases = dto.ChronicDiseases,
                    Allergies = dto.Allergies,
                    CurrentMedications = dto.CurrentMedications,
                    PreviousDentalSurgeries = dto.PreviousDentalSurgeries,
                    AdditionalNotes = dto.AdditionalNotes,
                    CreatedAt = DateTime.UtcNow
                };
                _context.MedicalHistories.Add(medicalHistory);
            }
            else
            {
                medicalHistory.ChronicDiseases = dto.ChronicDiseases;
                medicalHistory.Allergies = dto.Allergies;
                medicalHistory.CurrentMedications = dto.CurrentMedications;
                medicalHistory.PreviousDentalSurgeries = dto.PreviousDentalSurgeries;
                medicalHistory.AdditionalNotes = dto.AdditionalNotes;
                medicalHistory.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return (true, "Medical history updated successfully");
        }

        public async Task<PaginatedAppointmentsResponseDtoP> GetPatientAppointmentsAsync(string patientId, string? status, int page, int pageSize)
        {
            var query = _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Where(a => a.PatientId == patientId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<AppointmentStatus>(status, true, out var appointmentStatus))
                {
                    query = query.Where(a => a.Status == appointmentStatus);
                }
            }

            var totalCount = await query.CountAsync();

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new PatientAppointmentDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    Type = a.Type.ToString(),
                    Status = a.Status.ToString(),
                    Doctor = new AppointmentDoctorDto
                    {
                        Id = a.Doctor.UserId,
                        FirstName = a.Doctor.User.FirstName,
                        LastName = a.Doctor.User.LastName,
                        Specialization = a.Doctor.Specialization
                    },
                    ReasonForVisit = a.ReasonForVisit
                })
                .ToListAsync();

            return new PaginatedAppointmentsResponseDtoP
            {
                Appointments = appointments,
                TotalCount = totalCount
            };
        }
  
        public async Task<PaginatedMedicalRecordsResponseDto> GetPatientMedicalRecordsAsync(string patientId, int page, int pageSize)
        {
            var query = _context.MedicalRecords
                .Include(m => m.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User)
                .Include(m => m.Diagnoses)
                .Include(m => m.Treatments)
                .Where(m => m.PatientId == patientId);

            var totalCount = await query.CountAsync();

            var records = await query
                .OrderByDescending(m => m.VisitDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new PatientMedicalRecordDto
                {
                    Id = m.Id,
                    VisitDate = m.VisitDate,
                    ChiefComplaint = m.ChiefComplaint,
                    ExaminationNotes = m.ExaminationNotes,
                    Doctor = new RecordDoctorDto
                    {
                        Id = m.Appointment.Doctor.UserId,
                        FirstName = m.Appointment.Doctor.User.FirstName,
                        LastName = m.Appointment.Doctor.User.LastName
                    },
                    Diagnoses = m.Diagnoses.Select(d => new RecordDiagnosisDto
                    {
                        Id = d.Id,
                        DiagnosisName = d.DiagnosisName,
                        Severity = d.Severity.ToString()
                    }).ToList(),
                    Treatments = m.Treatments.Select(t => new RecordTreatmentDto
                    {
                        Id = t.Id,
                        TreatmentName = t.TreatmentName,
                        Status = t.Status.ToString()
                    }).ToList()
                })
                .ToListAsync();

            return new PaginatedMedicalRecordsResponseDto
            {
                Records = records,
                TotalCount = totalCount
            };
        }

        public async Task<PaginatedDiagnosesResponseDto> GetPatientDiagnosesAsync(string patientId, string? status, int page, int pageSize)
        {
            var query = _context.Diagnoses
                .Include(d => d.Doctor)
                    .ThenInclude(doc => doc.User)
                .Where(d => d.PatientId == patientId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<DiagnosisStatus>(status, true, out var diagnosisStatus))
                {
                    query = query.Where(d => d.Status == diagnosisStatus);
                }
            }

            var totalCount = await query.CountAsync();

            var diagnoses = await query
                .OrderByDescending(d => d.DiagnosisDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new PatientDiagnosisDto
                {
                    Id = d.Id,
                    DiagnosisName = d.DiagnosisName,
                    Description = d.Description,
                    Severity = d.Severity.ToString(),
                    Status = d.Status.ToString(),
                    DiagnosisDate = d.DiagnosisDate,
                    Doctor = new DiagnosisDoctorDto
                    {
                        FirstName = d.Doctor.User.FirstName,
                        LastName = d.Doctor.User.LastName
                    },
                    IsAIAssisted = d.IsAIAssisted,
                    AffectedArea = d.AffectedArea,
                    ToothNumber = d.ToothNumber
                })
                .ToListAsync();

            return new PaginatedDiagnosesResponseDto
            {
                Diagnoses = diagnoses,
                TotalCount = totalCount
            };
        }

        public async Task<PaginatedTreatmentsResponseDto> GetPatientTreatmentsAsync(string patientId, string? status, int page, int pageSize)
        {
            var query = _context.Treatments
                .Include(t => t.Doctor)
                    .ThenInclude(d => d.User)
                .Where(t => t.PatientId == patientId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<TreatmentStatus>(status, true, out var treatmentStatus))
                {
                    query = query.Where(t => t.Status == treatmentStatus);
                }
            }

            var totalCount = await query.CountAsync();

            var treatments = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new PatientTreatmentDto
                {
                    Id = t.Id,
                    TreatmentName = t.TreatmentName,
                    Description = t.Description,
                    Status = t.Status.ToString(),
                    StartDate = t.StartDate,
                    ExpectedCompletionDate = t.ExpectedCompletionDate,
                    TotalSessions = t.TotalSessions,
                    CompletedSessions = t.CompletedSessions,
                    Doctor = new TreatmentDoctorDto
                    {
                        FirstName = t.Doctor.User.FirstName,
                        LastName = t.Doctor.User.LastName
                    },
                    EstimatedCost = t.EstimatedCost,
                    PaidAmount = t.PaidAmount,
                    BalanceAmount = t.BalanceAmount
                })
                .ToListAsync();

            return new PaginatedTreatmentsResponseDto
            {
                Treatments = treatments,
                TotalCount = totalCount
            };
        }

        public async Task<PaginatedImagesResponseDto> GetPatientImagesAsync(string patientId, string? imageType, bool? isAnalyzedByAI, int page, int pageSize)
        {
            var query = _context.PatientImages
                .Where(i => i.PatientId == patientId);

            if (!string.IsNullOrWhiteSpace(imageType))
            {
                if (Enum.TryParse<ImageType>(imageType, true, out var imgType))
                {
                    query = query.Where(i => i.ImageType == imgType);
                }
            }

            if (isAnalyzedByAI.HasValue)
            {
                query = query.Where(i => i.IsAnalyzedByAI == isAnalyzedByAI.Value);
            }

            var totalCount = await query.CountAsync();

            var images = await query
                .OrderByDescending(i => i.UploadedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new PatientImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    ImageType = i.ImageType.ToString(),
                    ToothNumber = i.ToothNumber,
                    Notes = i.Notes,
                    UploadedAt = i.UploadedAt,
                    UploadedBy = new ImageUploaderDto
                    {
                        FirstName = i.Patient.User.FirstName,
                        LastName = i.Patient.User.LastName
                    },
                    IsAnalyzedByAI = i.IsAnalyzedByAI,
                    FileName = i.FileName,
                    FileSizeBytes = i.FileSizeBytes
                })
                .ToListAsync();

            return new PaginatedImagesResponseDto
            {
                Images = images,
                TotalCount = totalCount
            };
        }

        public async Task<PaginatedPrescriptionsResponseDto> GetPatientPrescriptionsAsync(string patientId, string? status, int page, int pageSize)
        {
            var query = _context.Prescriptions
                .Include(p => p.Doctor)
                    .ThenInclude(d => d.User)
                .Include(p => p.Medications)
                .Where(p => p.PatientId == patientId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<PrescriptionStatus>(status, true, out var prescriptionStatus))
                {
                    query = query.Where(p => p.Status == prescriptionStatus);
                }
            }

            var totalCount = await query.CountAsync();

            var prescriptions = await query
                .OrderByDescending(p => p.IssuedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PatientPrescriptionDto
                {
                    Id = p.Id,
                    IssuedDate = p.IssuedDate,
                    Status = p.Status.ToString(),
                    Doctor = new PrescriptionDoctorDto
                    {
                        FirstName = p.Doctor.User.FirstName,
                        LastName = p.Doctor.User.LastName
                    },
                    Medications = p.Medications.Select(m => new PrescriptionMedicationDto
                    {
                        MedicationName = m.MedicationName,
                        Dosage = m.Dosage,
                        Frequency = m.Frequency,
                        Duration = m.Duration
                    }).ToList(),
                    Notes = p.Notes
                })
                .ToListAsync();

            return new PaginatedPrescriptionsResponseDto
            {
                Prescriptions = prescriptions,
                TotalCount = totalCount
            };
        }

        public async Task<PaginatedDocumentsResponseDto> GetPatientDocumentsAsync(string patientId, string? documentType, int page, int pageSize)
        {
            var query = _context.Documents
                .Where(d => d.PatientId == patientId);

            if (!string.IsNullOrWhiteSpace(documentType))
            {
                if (Enum.TryParse<DocumentType>(documentType, true, out var docType))
                {
                    query = query.Where(d => d.DocumentType == docType);
                }
            }

            var totalCount = await query.CountAsync();

            var documents = await query
                .OrderByDescending(d => d.UploadedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new PatientDocumentDto
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    FileUrl = d.FileUrl,
                    DocumentType = d.DocumentType.ToString(),
                    FileSizeBytes = d.FileSizeBytes,
                    UploadedAt = d.UploadedAt,
                    UploadedBy = new DocumentUploaderDto
                    {
                        FirstName = d.Patient.User.FirstName,
                        LastName = d.Patient.User.LastName
                    }
                })
                .ToListAsync();

            return new PaginatedDocumentsResponseDto
            {
                Documents = documents,
                TotalCount = totalCount
            };
        }

        public async Task<(bool Success, string Message)> AssignDoctorToPatientAsync(string patientId, string doctorId)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == patientId);

            if (patient == null)
                return (false, "Patient not found");

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == doctorId);

            if (doctor == null)
                return (false, "Doctor not found");

            var oldDoctorId = patient.AssignedDoctorId;

            patient.AssignedDoctorId = doctorId;
            await _context.SaveChangesAsync();

            // Update old doctor's patient count
            if (!string.IsNullOrWhiteSpace(oldDoctorId) && oldDoctorId != doctorId)
            {
                var oldDoctor = await _context.Doctors.FindAsync(oldDoctorId);
                if (oldDoctor != null && oldDoctor.TotalPatients > 0)
                {
                    oldDoctor.TotalPatients--;
                }
            }

            // Update new doctor's patient count
            if (oldDoctorId != doctorId)
            {
                doctor.TotalPatients++;
            }

            await _context.SaveChangesAsync();
            return (true, "Doctor assigned successfully");
        }

        public async Task<PatientStatisticsDto> GetPatientsStatisticsAsync()
        {
            var totalPatients = await _context.Patients.CountAsync();

            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var newPatientsThisMonth = await _context.Patients
                .Include(p => p.User)
                .Where(p => p.User.CreatedAt >= startOfMonth)
                .CountAsync();

            var activePatients = await _context.Patients
                .Include(p => p.User)
                .Where(p => p.User.IsActive)
                .CountAsync();

            var inactivePatients = totalPatients - activePatients;

            var patientsWithUpcomingAppointments = await _context.Appointments
                .Where(a => a.AppointmentDate >= DateTime.UtcNow &&
                           a.Status != AppointmentStatus.Cancelled)
                .Select(a => a.PatientId)
                .Distinct()
                .CountAsync();

            return new PatientStatisticsDto
            {
                TotalPatients = totalPatients,
                NewPatientsThisMonth = newPatientsThisMonth,
                ActivePatients = activePatients,
                InactivePatients = inactivePatients,
                PatientsWithUpcomingAppointments = patientsWithUpcomingAppointments
            };
        }
    }
}

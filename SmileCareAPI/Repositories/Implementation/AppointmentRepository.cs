using Microsoft.EntityFrameworkCore;
using SmileCareAPI.Data;
using SmileCareAPI.DTOs;
using SmileCareAPI.Models;
using SmileCareAPI.Repositories.Interface;

namespace SmileCareAPI.Repositories.Implementation
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AppointmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AppointmentListResponseAPPDto> GetAllAppointmentsAsync(
            AppointmentQueryParametersAPPDto query,
            string currentUserId,
            UserRole currentUserRole)
        {
            var queryable = _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .AsQueryable();

            // Apply role-based filtering
            queryable = currentUserRole switch
            {
                UserRole.Doctor => queryable.Where(a => a.DoctorId == currentUserId),
                UserRole.Patient => queryable.Where(a => a.PatientId == currentUserId),
                _ => queryable // Admin and Receptionist see all
            };

            // Apply filters
            if (query.Date.HasValue)
                queryable = queryable.Where(a => a.AppointmentDate.Date == query.Date.Value.Date);

            if (!string.IsNullOrEmpty(query.DoctorId))
                queryable = queryable.Where(a => a.DoctorId == query.DoctorId);

            if (!string.IsNullOrEmpty(query.PatientId))
                queryable = queryable.Where(a => a.PatientId == query.PatientId);

            if (query.Status.HasValue)
                queryable = queryable.Where(a => a.Status == query.Status.Value);

            if (query.Type.HasValue)
                queryable = queryable.Where(a => a.Type == query.Type.Value);

            var totalCount = await queryable.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            var appointments = await queryable
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(a => new AppointmentListItemAPPDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    DurationMinutes = a.DurationMinutes,
                    Type = a.Type,
                    TypeName = a.Type.ToString(),
                    Status = a.Status,
                    StatusName = a.Status.ToString(),
                    ReasonForVisit = a.ReasonForVisit,
                    CreatedAt = a.CreatedAt,
                    Patient = new PatientBasicAPPDto
                    {
                        Id = a.Patient.UserId,
                        FirstName = a.Patient.User.FirstName,
                        LastName = a.Patient.User.LastName,
                        FullName = $"{a.Patient.User.FirstName} {a.Patient.User.LastName}",
                        PhoneNumber = a.Patient.User.PhoneNumber,
                        ProfilePicture = a.Patient.User.ProfilePicture
                    },
                    Doctor = new DoctorBasicAPPDto
                    {
                        Id = a.Doctor.UserId,
                        FirstName = a.Doctor.User.FirstName,
                        LastName = a.Doctor.User.LastName,
                        FullName = $"{a.Doctor.User.FirstName} {a.Doctor.User.LastName}",
                        Specialization = a.Doctor.Specialization,
                        ProfilePicture = a.Doctor.User.ProfilePicture
                    }
                })
                .ToListAsync();

            return new AppointmentListResponseAPPDto
            {
                Appointments = appointments,
                TotalCount = totalCount,
                CurrentPage = query.Page,
                TotalPages = totalPages
            };
        }

        public async Task<AppointmentDetailAPPDto?> GetAppointmentByIdAsync(
            int id,
            string currentUserId,
            UserRole currentUserRole)
        {
            var queryable = _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.MedicalRecord)
                .Where(a => a.Id == id);

            // Apply role-based filtering
            queryable = currentUserRole switch
            {
                UserRole.Doctor => queryable.Where(a => a.DoctorId == currentUserId),
                UserRole.Patient => queryable.Where(a => a.PatientId == currentUserId),
                _ => queryable
            };

            var appointment = await queryable.FirstOrDefaultAsync();

            if (appointment == null)
                return null;

            User? createdByUser = null;
            if (!string.IsNullOrEmpty(appointment.CreatedBy))
            {
                createdByUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == appointment.CreatedBy);
            }

            return new AppointmentDetailAPPDto
            {
                Id = appointment.Id,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                DurationMinutes = appointment.DurationMinutes,
                Type = appointment.Type,
                TypeName = appointment.Type.ToString(),
                Status = appointment.Status,
                StatusName = appointment.Status.ToString(),
                ReasonForVisit = appointment.ReasonForVisit,
                CancellationReason = appointment.CancellationReason,
                ConfirmationSentSms = appointment.ConfirmationSentSms,
                ConfirmationSentEmail = appointment.ConfirmationSentEmail,
                ReminderSent = appointment.ReminderSent,
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt,
                Patient = new PatientDetailAPPDto
                {
                    Id = appointment.Patient.UserId,
                    FirstName = appointment.Patient.User.FirstName,
                    LastName = appointment.Patient.User.LastName,
                    FullName = $"{appointment.Patient.User.FirstName} {appointment.Patient.User.LastName}",
                    Email = appointment.Patient.User.Email,
                    PhoneNumber = appointment.Patient.User.PhoneNumber,
                    DateOfBirth = appointment.Patient.User.DateOfBirth,
                    Age = DateTime.Now.Year - appointment.Patient.User.DateOfBirth.Year,
                    Gender = appointment.Patient.User.Gender,
                    GenderName = appointment.Patient.User.Gender.ToString(),
                    ProfilePicture = appointment.Patient.User.ProfilePicture
                },
                Doctor = new DoctorDetailAPPDto
                {
                    Id = appointment.Doctor.UserId,
                    FirstName = appointment.Doctor.User.FirstName,
                    LastName = appointment.Doctor.User.LastName,
                    FullName = $"{appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
                    Specialization = appointment.Doctor.Specialization,
                    PhoneNumber = appointment.Doctor.User.PhoneNumber,
                    Email = appointment.Doctor.User.Email,
                    ProfilePicture = appointment.Doctor.User.ProfilePicture
                },
                MedicalRecord = appointment.MedicalRecord != null ? new MedicalRecordBasicAPPDto
                {
                    Id = appointment.MedicalRecord.Id,
                    ChiefComplaint = appointment.MedicalRecord.ChiefComplaint,
                    ExaminationNotes = appointment.MedicalRecord.ExaminationNotes
                } : null,
                CreatedBy = createdByUser != null ? new CreatedByAPPDto
                {
                    FirstName = createdByUser.FirstName,
                    LastName = createdByUser.LastName,
                    FullName = $"{createdByUser.FirstName} {createdByUser.LastName}"
                } : null
            };
        }

        public async Task<int> CreateAppointmentAsync(CreateAppointmentAPPDto dto, string createdBy)
        {
            var appointment = new Models.Appointment
            {
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                AppointmentDate = dto.AppointmentDate,
                StartTime = dto.StartTime,
                DurationMinutes = dto.DurationMinutes,
                Type = dto.Type,
                Status = AppointmentStatus.Pending,
                ReasonForVisit = dto.ReasonForVisit,
                ConfirmationSentSms = dto.SendSmsConfirmation,
                ConfirmationSentEmail = dto.SendEmailConfirmation,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return appointment.Id;
        }

        public async Task<bool> UpdateAppointmentAsync(
            int id,
            UpdateAppointmentAPPDto dto,
            string currentUserId,
            UserRole currentUserRole)
        {
            var appointment = await GetAppointmentForUpdateAsync(id, currentUserId, currentUserRole);
            if (appointment == null)
                return false;

            if (appointment.Status == AppointmentStatus.Completed ||
                appointment.Status == AppointmentStatus.Cancelled)
                return false;

            if (dto.AppointmentDate.HasValue)
                appointment.AppointmentDate = dto.AppointmentDate.Value;

            if (dto.StartTime.HasValue)
                appointment.StartTime = dto.StartTime.Value;

            if (dto.DurationMinutes.HasValue)
                appointment.DurationMinutes = dto.DurationMinutes.Value;

            if (dto.Type.HasValue)
                appointment.Type = dto.Type.Value;

            if (dto.ReasonForVisit != null)
                appointment.ReasonForVisit = dto.ReasonForVisit;

            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAppointmentAsync(
            int id,
            string currentUserId,
            UserRole currentUserRole)
        {
            var appointment = await GetAppointmentForUpdateAsync(id, currentUserId, currentUserRole);
            if (appointment == null)
                return false;

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ConfirmAppointmentAsync(
            int id,
            ConfirmAppointmentAPPDto dto,
            string currentUserId,
            UserRole currentUserRole)
        {
            var appointment = await GetAppointmentForUpdateAsync(id, currentUserId, currentUserRole);
            if (appointment == null)
                return false;

            if (appointment.Status != AppointmentStatus.Pending)
                return false;

            appointment.Status = AppointmentStatus.Confirmed;
            appointment.UpdatedAt = DateTime.UtcNow;

            if (dto.SendConfirmation)
            {
                appointment.ConfirmationSentSms = true;
                appointment.ConfirmationSentEmail = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelAppointmentAsync(
            int id,
            CancelAppointmentAPPDto dto,
            string currentUserId,
            UserRole currentUserRole)
        {
            var appointment = await GetAppointmentForUpdateAsync(id, currentUserId, currentUserRole);
            if (appointment == null)
                return false;

            if (appointment.Status == AppointmentStatus.Completed ||
                appointment.Status == AppointmentStatus.Cancelled)
                return false;

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.CancellationReason = dto.CancellationReason;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteAppointmentAsync(
            int id,
            string currentUserId,
            UserRole currentUserRole)
        {
            var appointment = await GetAppointmentForUpdateAsync(id, currentUserId, currentUserRole);
            if (appointment == null)
                return false;

            if (appointment.Status != AppointmentStatus.Confirmed)
                return false;

            appointment.Status = AppointmentStatus.Completed;
            appointment.UpdatedAt = DateTime.UtcNow;

            // Update doctor statistics
            var doctor = await _context.Doctors.FindAsync(appointment.DoctorId);
            if (doctor != null)
            {
                doctor.CompletedAppointments++;
            }

            // Update patient statistics
            var patient = await _context.Patients.FindAsync(appointment.PatientId);
            if (patient != null)
            {
                patient.TotalVisits++;
                patient.LastVisitDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAsNoShowAsync(
            int id,
            string currentUserId,
            UserRole currentUserRole)
        {
            var appointment = await GetAppointmentForUpdateAsync(id, currentUserId, currentUserRole);
            if (appointment == null)
                return false;

            if (appointment.Status != AppointmentStatus.Confirmed)
                return false;

            appointment.Status = AppointmentStatus.NoShow;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RescheduleAppointmentAsync(
            int id,
            RescheduleAppointmentAPPDto dto,
            string currentUserId,
            UserRole currentUserRole)
        {
            var appointment = await GetAppointmentForUpdateAsync(id, currentUserId, currentUserRole);
            if (appointment == null)
                return false;

            if (appointment.Status == AppointmentStatus.Completed ||
                appointment.Status == AppointmentStatus.Cancelled)
                return false;

            appointment.AppointmentDate = dto.NewDate;
            appointment.StartTime = dto.NewStartTime;
            appointment.UpdatedAt = DateTime.UtcNow;

            if (appointment.Status == AppointmentStatus.Confirmed)
            {
                appointment.Status = AppointmentStatus.Pending;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SendReminderAsync(
            int id,
            SendReminderAPPDto dto,
            string currentUserId,
            UserRole currentUserRole)
        {
            var appointment = await GetAppointmentForUpdateAsync(id, currentUserId, currentUserRole);
            if (appointment == null)
                return false;

            appointment.ReminderSent = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AppointmentListResponseAPPDto> GetTodayAppointmentsAsync(
            string currentUserId,
            UserRole currentUserRole,
            int page = 1,
            int pageSize = 20)
        {
            var today = DateTime.Today;
            var query = new AppointmentQueryParametersAPPDto
            {
                Date = today,
                Page = page,
                PageSize = pageSize
            };

            return await GetAllAppointmentsAsync(query, currentUserId, currentUserRole);
        }

        public async Task<AppointmentListResponseAPPDto> GetUpcomingAppointmentsAsync(
            UpcomingAppointmentsQueryAPPDto query,
            string currentUserId,
            UserRole currentUserRole)
        {
            var today = DateTime.Today;
            var endDate = today.AddDays(query.Days);

            var queryable = _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Where(a => a.AppointmentDate >= today && a.AppointmentDate <= endDate)
                .Where(a => a.Status != AppointmentStatus.Cancelled &&
                           a.Status != AppointmentStatus.Completed)
                .AsQueryable();

            // Apply role-based filtering
            queryable = currentUserRole switch
            {
                UserRole.Doctor => queryable.Where(a => a.DoctorId == currentUserId),
                UserRole.Patient => queryable.Where(a => a.PatientId == currentUserId),
                _ => queryable
            };

            var totalCount = await queryable.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            var appointments = await queryable
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(a => new AppointmentListItemAPPDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    DurationMinutes = a.DurationMinutes,
                    Type = a.Type,
                    TypeName = a.Type.ToString(),
                    Status = a.Status,
                    StatusName = a.Status.ToString(),
                    ReasonForVisit = a.ReasonForVisit,
                    CreatedAt = a.CreatedAt,
                    Patient = new PatientBasicAPPDto
                    {
                        Id = a.Patient.UserId,
                        FirstName = a.Patient.User.FirstName,
                        LastName = a.Patient.User.LastName,
                        FullName = $"{a.Patient.User.FirstName} {a.Patient.User.LastName}",
                        PhoneNumber = a.Patient.User.PhoneNumber,
                        ProfilePicture = a.Patient.User.ProfilePicture
                    },
                    Doctor = new DoctorBasicAPPDto
                    {
                        Id = a.Doctor.UserId,
                        FirstName = a.Doctor.User.FirstName,
                        LastName = a.Doctor.User.LastName,
                        FullName = $"{a.Doctor.User.FirstName} {a.Doctor.User.LastName}",
                        Specialization = a.Doctor.Specialization,
                        ProfilePicture = a.Doctor.User.ProfilePicture
                    }
                })
                .ToListAsync();

            return new AppointmentListResponseAPPDto
            {
                Appointments = appointments,
                TotalCount = totalCount,
                CurrentPage = query.Page,
                TotalPages = totalPages
            };
        }

        public async Task<AvailableSlotsResponseAPPDto> GetAvailableSlotsAsync(
            AvailableSlotsQueryAPPDto query)
        {
            var workingHours = await _context.WorkingHours
                .Where(w => w.DoctorId == query.DoctorId &&
                           w.DayOfWeek == query.Date.DayOfWeek &&
                           w.IsOpen)
                .FirstOrDefaultAsync();

            var slots = new List<AvailableSlotAPPDto>();

            if (workingHours == null || !workingHours.OpenTime.HasValue || !workingHours.CloseTime.HasValue)
            {
                return new AvailableSlotsResponseAPPDto
                {
                    Date = query.Date,
                    DoctorId = query.DoctorId,
                    Slots = slots
                };
            }

            // Get existing appointments for this doctor on this date
            var existingAppointments = await _context.Appointments
                .Where(a => a.DoctorId == query.DoctorId &&
                           a.AppointmentDate.Date == query.Date.Date &&
                           a.Status != AppointmentStatus.Cancelled)
                .Select(a => new { a.StartTime, a.DurationMinutes })
                .ToListAsync();

            var currentTime = workingHours.OpenTime.Value;
            var closeTime = workingHours.CloseTime.Value;
            var slotDuration = TimeSpan.FromMinutes(30); // Default slot duration

            while (currentTime.Add(slotDuration) <= closeTime)
            {
                // Check if slot is during break time
                var isDuringBreak = false;
                if (workingHours.BreakStartTime.HasValue && workingHours.BreakEndTime.HasValue)
                {
                    isDuringBreak = currentTime >= workingHours.BreakStartTime.Value &&
                                   currentTime < workingHours.BreakEndTime.Value;
                }

                // Check if slot is already booked
                var isBooked = existingAppointments.Any(apt =>
                {
                    var aptEndTime = apt.StartTime.Add(TimeSpan.FromMinutes(apt.DurationMinutes));
                    var slotEndTime = currentTime.Add(slotDuration);
                    return (currentTime >= apt.StartTime && currentTime < aptEndTime) ||
                           (slotEndTime > apt.StartTime && slotEndTime <= aptEndTime) ||
                           (currentTime <= apt.StartTime && slotEndTime >= aptEndTime);
                });

                slots.Add(new AvailableSlotAPPDto
                {
                    StartTime = currentTime,
                    EndTime = currentTime.Add(slotDuration),
                    IsAvailable = !isDuringBreak && !isBooked
                });

                currentTime = currentTime.Add(slotDuration);
            }

            return new AvailableSlotsResponseAPPDto
            {
                Date = query.Date,
                DoctorId = query.DoctorId,
                Slots = slots
            };
        }

        public async Task<AppointmentStatisticsAPPDto> GetStatisticsAsync(
            StatisticsQueryParametersAPPDto query,
            string currentUserId,
            UserRole currentUserRole)
        {
            var queryable = _context.Appointments.AsQueryable();

            // Apply role-based filtering
            queryable = currentUserRole switch
            {
                UserRole.Doctor => queryable.Where(a => a.DoctorId == currentUserId),
                _ => queryable
            };

            // Apply date range filter
            if (query.StartDate.HasValue)
                queryable = queryable.Where(a => a.AppointmentDate >= query.StartDate.Value);

            if (query.EndDate.HasValue)
                queryable = queryable.Where(a => a.AppointmentDate <= query.EndDate.Value);

            // Apply doctor filter
            if (!string.IsNullOrEmpty(query.DoctorId))
                queryable = queryable.Where(a => a.DoctorId == query.DoctorId);

            var appointments = await queryable.ToListAsync();

            var totalAppointments = appointments.Count;
            var pending = appointments.Count(a => a.Status == AppointmentStatus.Pending);
            var confirmed = appointments.Count(a => a.Status == AppointmentStatus.Confirmed);
            var completed = appointments.Count(a => a.Status == AppointmentStatus.Completed);
            var cancelled = appointments.Count(a => a.Status == AppointmentStatus.Cancelled);
            var noShow = appointments.Count(a => a.Status == AppointmentStatus.NoShow);

            var completionRate = totalAppointments > 0
                ? Math.Round((decimal)completed / totalAppointments * 100, 2)
                : 0;

            // Appointments by day
            var appointmentsByDay = appointments
                .GroupBy(a => a.AppointmentDate.Date)
                .Select(g => new AppointmentsByDayAPPDto
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Appointments by type
            var appointmentsByType = appointments
                .GroupBy(a => a.Type)
                .Select(g => new AppointmentsByTypeAPPDto
                {
                    Type = g.Key,
                    TypeName = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToList();

            // Appointments by doctor
            var doctorQueryable = _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .AsQueryable();

            if (query.StartDate.HasValue)
                doctorQueryable = doctorQueryable.Where(a => a.AppointmentDate >= query.StartDate.Value);

            if (query.EndDate.HasValue)
                doctorQueryable = doctorQueryable.Where(a => a.AppointmentDate <= query.EndDate.Value);

            if (!string.IsNullOrEmpty(query.DoctorId))
                doctorQueryable = doctorQueryable.Where(a => a.DoctorId == query.DoctorId);

            var appointmentsByDoctor = await doctorQueryable
                .GroupBy(a => new { a.DoctorId, a.Doctor.User.FirstName, a.Doctor.User.LastName })
                .Select(g => new AppointmentsByDoctorAPPDto
                {
                    DoctorName = $"{g.Key.FirstName} {g.Key.LastName}",
                    Count = g.Count()
                })
                .ToListAsync();

            return new AppointmentStatisticsAPPDto
            {
                TotalAppointments = totalAppointments,
                Pending = pending,
                Confirmed = confirmed,
                Completed = completed,
                Cancelled = cancelled,
                NoShow = noShow,
                CompletionRate = completionRate,
                AppointmentsByDay = appointmentsByDay,
                AppointmentsByType = appointmentsByType,
                AppointmentsByDoctor = appointmentsByDoctor
            };
        }

        public async Task<bool> IsSlotAvailableAsync(
           string doctorId,
           DateTime date,
           TimeSpan startTime,
           int durationMinutes,
           int? excludeAppointmentId = null)
        {
            var endTime = startTime.Add(TimeSpan.FromMinutes(durationMinutes));

            var conflictingAppointment = await _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                            a.AppointmentDate.Date == date.Date &&
                            a.Status != AppointmentStatus.Cancelled &&
                            (excludeAppointmentId == null || a.Id != excludeAppointmentId))
                .AnyAsync(a =>
                    (startTime >= a.StartTime && startTime < a.StartTime.Add(TimeSpan.FromMinutes(a.DurationMinutes))) ||
                    (endTime > a.StartTime && endTime <= a.StartTime.Add(TimeSpan.FromMinutes(a.DurationMinutes))) ||
                    (startTime <= a.StartTime && endTime >= a.StartTime.Add(TimeSpan.FromMinutes(a.DurationMinutes)))
                );

            return !conflictingAppointment;
        }


        public async Task<bool> HasConflictingAppointmentAsync(
      string patientId,
      DateTime date,
      TimeSpan startTime,
      int durationMinutes,
      int? excludeAppointmentId = null)
        {
            var endTime = startTime.Add(TimeSpan.FromMinutes(durationMinutes));

            var conflictingAppointment = await _context.Appointments
                .Where(a => a.PatientId == patientId &&
                            a.AppointmentDate.Date == date.Date &&
                            a.Status != AppointmentStatus.Cancelled &&
                            (excludeAppointmentId == null || a.Id != excludeAppointmentId))
                .AnyAsync(a =>
                    (startTime >= a.StartTime && startTime < a.StartTime.Add(TimeSpan.FromMinutes(a.DurationMinutes))) ||
                    (endTime > a.StartTime && endTime <= a.StartTime.Add(TimeSpan.FromMinutes(a.DurationMinutes))) ||
                    (startTime <= a.StartTime && endTime >= a.StartTime.Add(TimeSpan.FromMinutes(a.DurationMinutes)))
                );

            return conflictingAppointment;
        }


        public async Task<bool> IsDoctorAvailableAsync(
            string doctorId,
            DateTime date,
            TimeSpan startTime)
        {
            var workingHours = await _context.WorkingHours
                .Where(w => w.DoctorId == doctorId &&
                           w.DayOfWeek == date.DayOfWeek &&
                           w.IsOpen)
                .FirstOrDefaultAsync();

            if (workingHours == null || !workingHours.OpenTime.HasValue || !workingHours.CloseTime.HasValue)
                return false;

            // Check if time is within working hours
            if (startTime < workingHours.OpenTime.Value || startTime >= workingHours.CloseTime.Value)
                return false;

            // Check if time is during break
            if (workingHours.BreakStartTime.HasValue && workingHours.BreakEndTime.HasValue)
            {
                if (startTime >= workingHours.BreakStartTime.Value &&
                    startTime < workingHours.BreakEndTime.Value)
                    return false;
            }

            // Check for holidays
            var isHoliday = await _context.WorkingHours
                .AnyAsync(w => w.HolidayDate.HasValue &&
                              w.HolidayDate.Value.Date == date.Date &&
                              (w.DoctorId == null || w.DoctorId == doctorId));

            if (isHoliday)
                return false;

            // Check global holidays table if exists
            var isGlobalHoliday = await _context.Set<Holiday>()
                .AnyAsync(h => h.Date.Date == date.Date && h.IsClosed);

            return !isGlobalHoliday;
        }

        private async Task<Models.Appointment?> GetAppointmentForUpdateAsync(
            int id,
            string currentUserId,
            UserRole currentUserRole)
        {
            var queryable = _context.Appointments.AsQueryable();

            // Apply role-based filtering
            queryable = currentUserRole switch
            {
                UserRole.Doctor => queryable.Where(a => a.DoctorId == currentUserId),
                UserRole.Patient => queryable.Where(a => a.PatientId == currentUserId),
                _ => queryable
            };

            return await queryable.FirstOrDefaultAsync(a => a.Id == id);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using SmileCareAPI.Data;
using SmileCareAPI.DTOs;
using SmileCareAPI.Models;
using SmileCareAPI.Repositories.Interface;

namespace SmileCareAPI.Repositories.Implementation
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly ApplicationDbContext _context;

        public DoctorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<DoctorListDto> Doctors, int TotalCount)> GetAllDoctorsAsync(
            string? search,
            string? specialization,
            int page,
            int pageSize)
        {
            var query = _context.Doctors
                .Include(d => d.User)
                .Where(d => d.User.IsActive)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(d =>
                    d.User.FirstName.ToLower().Contains(search) ||
                    d.User.LastName.ToLower().Contains(search) ||
                    d.User.Email.ToLower().Contains(search) ||
                    d.Specialization.ToLower().Contains(search));
            }

            // Apply specialization filter
            if (!string.IsNullOrWhiteSpace(specialization))
            {
                query = query.Where(d => d.Specialization.ToLower() == specialization.ToLower());
            }

            var totalCount = await query.CountAsync();

            var doctors = await query
                .OrderBy(d => d.User.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DoctorListDto
                {
                    Id = d.UserId,
                    FirstName = d.User.FirstName,
                    LastName = d.User.LastName,
                    Email = d.User.Email,
                    PhoneNumber = d.User.PhoneNumber,
                    Specialization = d.Specialization,
                    LicenseNumber = d.LicenseNumber,
                    YearsOfExperience = d.YearsOfExperience,
                    AverageRating = d.AverageRating,
                    TotalPatients = d.TotalPatients,
                    CompletedAppointments = d.CompletedAppointments,
                    ProfilePicture = d.User.ProfilePicture,
                    IsActive = d.User.IsActive
                })
                .ToListAsync();

            return (doctors, totalCount);
        }

        public async Task<DoctorDetailsDto?> GetDoctorByIdAsync(string doctorId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.WorkingHours)
                .Where(d => d.UserId == doctorId)
                .FirstOrDefaultAsync();

            if (doctor == null)
                return null;

            var statistics = await GetDoctorStatisticsAsync(doctorId);

            return new DoctorDetailsDto
            {
                User = new UserInfoDto
                {
                    Id = doctor.User.Id,
                    FirstName = doctor.User.FirstName,
                    LastName = doctor.User.LastName,
                    Email = doctor.User.Email,
                    PhoneNumber = doctor.User.PhoneNumber,
                    ProfilePicture = doctor.User.ProfilePicture
                },
                Doctor = new DoctorInfoDto
                {
                    Specialization = doctor.Specialization,
                    LicenseNumber = doctor.LicenseNumber,
                    YearsOfExperience = doctor.YearsOfExperience,
                    Credentials = doctor.Credentials,
                    AverageRating = doctor.AverageRating,
                    TotalPatients = doctor.TotalPatients,
                    CompletedAppointments = doctor.CompletedAppointments
                },
                WorkingHours = doctor.WorkingHours
                    .Where(wh => wh.HolidayDate == null)
                    .Select(wh => new WorkingHoursDto
                    {
                        Id = wh.Id,
                        DayOfWeek = wh.DayOfWeek,
                        IsOpen = wh.IsOpen,
                        OpenTime = wh.OpenTime,
                        CloseTime = wh.CloseTime,
                        BreakStartTime = wh.BreakStartTime,
                        BreakEndTime = wh.BreakEndTime
                    })
                    .OrderBy(wh => wh.DayOfWeek)
                    .ToList(),
                Statistics = statistics
            };
        }

        public async Task<Doctor?> GetDoctorEntityAsync(string doctorId)
        {
            return await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == doctorId);
        }

        public async Task<(List<PatientListDto> Patients, int TotalCount)> GetDoctorPatientsAsync(
            string doctorId,
            int page,
            int pageSize)
        {
            var query = _context.Patients
                .Include(p => p.User)
                .Where(p => p.AssignedDoctorId == doctorId)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var patients = await query
                .OrderByDescending(p => p.LastVisitDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PatientListDto
                {
                    Id = p.UserId,
                    FirstName = p.User.FirstName,
                    LastName = p.User.LastName,
                    Email = p.User.Email,
                    PhoneNumber = p.User.PhoneNumber,
                    DateOfBirth = p.User.DateOfBirth,
                    Gender = p.User.Gender,
                    TotalVisits = p.TotalVisits,
                    LastVisitDate = p.LastVisitDate,
                    ProfilePicture = p.User.ProfilePicture
                })
                .ToListAsync();

            return (patients, totalCount);
        }

        public async Task<(List<AppointmentListDto> Appointments, int TotalCount)> GetDoctorAppointmentsAsync(
            string doctorId,
            AppointmentStatus? status,
            DateTime? date,
            int page,
            int pageSize)
        {
            var query = _context.Appointments
                .Include(a => a.Patient)
                .ThenInclude(p => p.User)
                .Where(a => a.DoctorId == doctorId)
                .AsQueryable();

            // Apply status filter
            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            // Apply date filter
            if (date.HasValue)
            {
                query = query.Where(a => a.AppointmentDate.Date == date.Value.Date);
            }

            var totalCount = await query.CountAsync();

            var appointments = await query
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AppointmentListDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    DurationMinutes = a.DurationMinutes,
                    Type = a.Type,
                    Status = a.Status,
                    Patient = new PatientBasicInfoDto
                    {
                        Id = a.Patient.UserId,
                        FirstName = a.Patient.User.FirstName,
                        LastName = a.Patient.User.LastName,
                        ProfilePicture = a.Patient.User.ProfilePicture
                    },
                    ReasonForVisit = a.ReasonForVisit
                })
                .ToListAsync();

            return (appointments, totalCount);
        }

        public async Task<List<WorkingHoursDto>> GetDoctorWorkingHoursAsync(string doctorId)
        {
            return await _context.WorkingHours
                .Where(wh => wh.DoctorId == doctorId && wh.HolidayDate == null)
                .OrderBy(wh => wh.DayOfWeek)
                .Select(wh => new WorkingHoursDto
                {
                    Id = wh.Id,
                    DayOfWeek = wh.DayOfWeek,
                    IsOpen = wh.IsOpen,
                    OpenTime = wh.OpenTime,
                    CloseTime = wh.CloseTime,
                    BreakStartTime = wh.BreakStartTime,
                    BreakEndTime = wh.BreakEndTime
                })
                .ToListAsync();
        }

        public async Task<bool> UpdateDoctorWorkingHoursAsync(string doctorId, List<UpdateWorkingHoursDto> workingHours)
        {
            // Delete existing working hours for this doctor
            var existingHours = await _context.WorkingHours
                .Where(wh => wh.DoctorId == doctorId && wh.HolidayDate == null)
                .ToListAsync();

            _context.WorkingHours.RemoveRange(existingHours);

            // Add new working hours
            foreach (var wh in workingHours)
            {
                _context.WorkingHours.Add(new WorkingHours
                {
                    DoctorId = doctorId,
                    DayOfWeek = wh.DayOfWeek,
                    IsOpen = wh.IsOpen,
                    OpenTime = wh.OpenTime,
                    CloseTime = wh.CloseTime,
                    BreakStartTime = wh.BreakStartTime,
                    BreakEndTime = wh.BreakEndTime,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<AvailableSlotDto>> GetDoctorAvailableSlotsAsync(string doctorId, DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;

            // Get working hours for the day
            var workingHours = await _context.WorkingHours
                .FirstOrDefaultAsync(wh => wh.DoctorId == doctorId &&
                                          wh.DayOfWeek == dayOfWeek &&
                                          wh.HolidayDate == null &&
                                          wh.IsOpen);

            if (workingHours == null || !workingHours.OpenTime.HasValue || !workingHours.CloseTime.HasValue)
            {
                return new List<AvailableSlotDto>();
            }

            // Check if it's a holiday
            var isHoliday = await _context.WorkingHours
                .AnyAsync(wh => wh.DoctorId == doctorId &&
                               wh.HolidayDate.HasValue &&
                               wh.HolidayDate.Value.Date == date.Date);

            if (isHoliday)
            {
                return new List<AvailableSlotDto>();
            }

            // Get existing appointments for the date
            var appointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                           a.AppointmentDate.Date == date.Date &&
                           (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed))
                .Select(a => new { a.StartTime, a.DurationMinutes })
                .ToListAsync();

            var slots = new List<AvailableSlotDto>();
            var slotDuration = TimeSpan.FromMinutes(30); // Default slot duration
            var currentTime = workingHours.OpenTime.Value;

            while (currentTime < workingHours.CloseTime.Value)
            {
                var endTime = currentTime.Add(slotDuration);

                if (endTime > workingHours.CloseTime.Value)
                    break;

                // Check if slot is during break time
                var isDuringBreak = false;
                if (workingHours.BreakStartTime.HasValue && workingHours.BreakEndTime.HasValue)
                {
                    isDuringBreak = currentTime >= workingHours.BreakStartTime.Value &&
                                   currentTime < workingHours.BreakEndTime.Value;
                }

                // Check if slot is already booked
                var isBooked = appointments.Any(a =>
                {
                    var appointmentEnd = a.StartTime.Add(TimeSpan.FromMinutes(a.DurationMinutes));
                    return (currentTime >= a.StartTime && currentTime < appointmentEnd) ||
                           (endTime > a.StartTime && endTime <= appointmentEnd);
                });

                slots.Add(new AvailableSlotDto
                {
                    StartTime = currentTime,
                    EndTime = endTime,
                    IsAvailable = !isBooked && !isDuringBreak
                });

                currentTime = endTime;
            }

            return slots;
        }

        public async Task<bool> AddDoctorHolidayAsync(string doctorId, DateTime holidayDate, string holidayName)
        {
            var holiday = new WorkingHours
            {
                DoctorId = doctorId,
                HolidayDate = holidayDate,
                HolidayName = holidayName,
                IsOpen = false,
                DayOfWeek = DayOfWeek.Sunday, // Not used for holidays
                UpdatedAt = DateTime.UtcNow
            };

            _context.WorkingHours.Add(holiday);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<DoctorHolidayDto>> GetDoctorHolidaysAsync(string doctorId)
        {
            return await _context.WorkingHours
                .Where(wh => wh.DoctorId == doctorId && wh.HolidayDate.HasValue)
                .OrderBy(wh => wh.HolidayDate)
                .Select(wh => new DoctorHolidayDto
                {
                    Id = wh.Id,
                    HolidayDate = wh.HolidayDate.Value,
                    HolidayName = wh.HolidayName ?? "Holiday"
                })
                .ToListAsync();
        }

        public async Task<bool> DeleteDoctorHolidayAsync(int holidayId, string doctorId)
        {
            var holiday = await _context.WorkingHours
                .FirstOrDefaultAsync(wh => wh.Id == holidayId &&
                                          wh.DoctorId == doctorId &&
                                          wh.HolidayDate.HasValue);

            if (holiday == null)
                return false;

            _context.WorkingHours.Remove(holiday);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<DoctorStatisticsDto> GetDoctorStatisticsAsync(string doctorId)
        {
            var today = DateTime.UtcNow.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var appointmentsToday = await _context.Appointments
                .CountAsync(a => a.DoctorId == doctorId &&
                                a.AppointmentDate.Date == today &&
                                (a.Status == AppointmentStatus.Pending ||
                                 a.Status == AppointmentStatus.Confirmed));

            var appointmentsThisWeek = await _context.Appointments
                .CountAsync(a => a.DoctorId == doctorId &&
                                a.AppointmentDate >= startOfWeek &&
                                (a.Status == AppointmentStatus.Pending ||
                                 a.Status == AppointmentStatus.Confirmed));

            var appointmentsThisMonth = await _context.Appointments
                .CountAsync(a => a.DoctorId == doctorId &&
                                a.AppointmentDate >= startOfMonth &&
                                (a.Status == AppointmentStatus.Pending ||
                                 a.Status == AppointmentStatus.Confirmed));

            return new DoctorStatisticsDto
            {
                AppointmentsToday = appointmentsToday,
                AppointmentsThisWeek = appointmentsThisWeek,
                AppointmentsThisMonth = appointmentsThisMonth
            };
        }

        public async Task<bool> DoctorExistsAsync(string doctorId)
        {
            return await _context.Doctors.AnyAsync(d => d.UserId == doctorId);
        }
    }
}

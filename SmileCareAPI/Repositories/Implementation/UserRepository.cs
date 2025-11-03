using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmileCareAPI.Data;
using SmileCareAPI.DTOs;
using SmileCareAPI.Models;
using SmileCareAPI.Repositories.Interface;

namespace SmileCareAPI.Repositories.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;

        public UserRepository(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        public async Task<UsersListResponseDto> GetAllUsersAsync(
            UserRole? role = null,
            bool? isActive = null,
            string? search = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.Users.AsQueryable();

            // Apply filters
            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(u =>
                    u.FirstName.ToLower().Contains(search) ||
                    u.LastName.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search) ||
                    u.PhoneNumber.Contains(search)
                );
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListItemDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Role = u.Role,
                    RoleName = u.Role.ToString(),
                    Gender = u.Gender,
                    GenderName = u.Gender.ToString(),
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    ProfilePicture = u.ProfilePicture
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new UsersListResponseDto
            {
                Users = users,
                TotalCount = totalCount,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };
        }

        public async Task<UserDetailDto?> GetUserByIdAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.Doctor)
                .Include(u => u.Patient)
                    .ThenInclude(p => p.AssignedDoctor)
                        .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            var userDetail = new UserDetailDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                RoleName = user.Role.ToString(),
                Gender = user.Gender,
                GenderName = user.Gender.ToString(),
                DateOfBirth = user.DateOfBirth,
                City = user.City,
                DetailedAddress = user.DetailedAddress,
                ProfilePicture = user.ProfilePicture,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            if (user.Role == UserRole.Doctor && user.Doctor != null)
            {
                userDetail.Doctor = new DoctorInfoDto
                {
                    Specialization = user.Doctor.Specialization,
                    LicenseNumber = user.Doctor.LicenseNumber,
                    YearsOfExperience = user.Doctor.YearsOfExperience,
                    Credentials = user.Doctor.Credentials,
                    AverageRating = user.Doctor.AverageRating,
                    TotalPatients = user.Doctor.TotalPatients,
                    CompletedAppointments = user.Doctor.CompletedAppointments
                };
            }

            if (user.Role == UserRole.Patient && user.Patient != null)
            {
                userDetail.Patient = new PatientInfoDto
                {
                    EmergencyContactName = user.Patient.EmergencyContactName,
                    EmergencyContactRelationship = user.Patient.EmergencyContactRelationship,
                    EmergencyContactPhone = user.Patient.EmergencyContactPhone,
                    EmergencyContactAlternativePhone = user.Patient.EmergencyContactAlternativePhone,
                    AssignedDoctorId = user.Patient.AssignedDoctorId,
                    AssignedDoctorName = user.Patient.AssignedDoctor != null
                        ? $"{user.Patient.AssignedDoctor.User.FirstName} {user.Patient.AssignedDoctor.User.LastName}"
                        : null,
                    BloodType = user.Patient.BloodType,
                    TotalVisits = user.Patient.TotalVisits,
                    LastVisitDate = user.Patient.LastVisitDate
                };
            }

            return userDetail;
        }

        public async Task<(bool Success, string? UserId, string? ErrorMessage)> CreateUserAsync(CreateUserDto dto)
        {
            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return (false, null, "Email already exists");
            }

            // Check if username already exists
            existingUser = await _userManager.FindByNameAsync(dto.Username);
            if (existingUser != null)
            {
                return (false, null, "Username already exists");
            }

            // Validate doctor-specific data
            if (dto.Role == UserRole.Doctor && string.IsNullOrWhiteSpace(dto.Specialization))
            {
                return (false, null, "Specialization is required for doctors");
            }

            // Validate assigned doctor if patient
            if (dto.Role == UserRole.Patient && !string.IsNullOrWhiteSpace(dto.AssignedDoctorId))
            {
                var doctorExists = await _context.Doctors.AnyAsync(d => d.UserId == dto.AssignedDoctorId);
                if (!doctorExists)
                {
                    return (false, null, "Assigned doctor not found");
                }
            }

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                UserName = dto.Username,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                Role = dto.Role,
                City = dto.City,
                DetailedAddress = dto.DetailedAddress,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return (false, null, string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            // Add user to role
            await _userManager.AddToRoleAsync(user, dto.Role.ToString());

            // Create Doctor record if role is Doctor
            if (dto.Role == UserRole.Doctor)
            {
                var doctor = new Doctor
                {
                    UserId = user.Id,
                    Specialization = dto.Specialization,
                    LicenseNumber = dto.LicenseNumber,
                    YearsOfExperience = dto.YearsOfExperience,
                    Credentials = dto.Credentials,
                    TotalPatients = 0,
                    CompletedAppointments = 0
                };
                _context.Doctors.Add(doctor);

            }
          
            // Create Patient record if role is Patient
            if (dto.Role == UserRole.Patient)
            {
                var patient = new Patient
                {
                    UserId = user.Id,
                    EmergencyContactName = dto.EmergencyContactName,
                    EmergencyContactRelationship = dto.EmergencyContactRelationship,
                    EmergencyContactPhone = dto.EmergencyContactPhone,
                    EmergencyContactAlternativePhone = dto.EmergencyContactAlternativePhone,
                    AssignedDoctorId = dto.AssignedDoctorId,
                    BloodType = dto.BloodType,
                    TotalVisits = 0
                };
                _context.Patients.Add(patient);


                // Create MedicalHistory record for patient
                var medicalHistory = new MedicalHistory
                {
                    PatientId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.MedicalHistories.Add(medicalHistory);
            }

            await _context.SaveChangesAsync();

            return (true, user.Id, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateUserAsync(string userId, UpdateUserDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Doctor)
                .Include(u => u.Patient)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return (false, "User not found");
            }

            // Validate assigned doctor if patient
            if (user.Role == UserRole.Patient && !string.IsNullOrWhiteSpace(dto.AssignedDoctorId))
            {
                var doctorExists = await _context.Doctors.AnyAsync(d => d.UserId == dto.AssignedDoctorId);
                if (!doctorExists)
                {
                    return (false, "Assigned doctor not found");
                }
            }

            // Update basic user info
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;
            user.Gender = dto.Gender;
            user.DateOfBirth = dto.DateOfBirth;
            user.City = dto.City;
            user.DetailedAddress = dto.DetailedAddress;

            // Update doctor-specific info
            if (user.Role == UserRole.Doctor && user.Doctor != null)
            {
                user.Doctor.Specialization = dto.Specialization ?? user.Doctor.Specialization;
                user.Doctor.LicenseNumber = dto.LicenseNumber;
                user.Doctor.YearsOfExperience = dto.YearsOfExperience;
                user.Doctor.Credentials = dto.Credentials;
            }

            // Update patient-specific info
            if (user.Role == UserRole.Patient && user.Patient != null)
            {
                user.Patient.EmergencyContactName = dto.EmergencyContactName;
                user.Patient.EmergencyContactRelationship = dto.EmergencyContactRelationship;
                user.Patient.EmergencyContactPhone = dto.EmergencyContactPhone;
                user.Patient.EmergencyContactAlternativePhone = dto.EmergencyContactAlternativePhone;
                user.Patient.AssignedDoctorId = dto.AssignedDoctorId;
                user.Patient.BloodType = dto.BloodType;
            }

            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            // Check if user has related data that prevents deletion
            if (user.Role == UserRole.Doctor)
            {
                var hasAppointments = await _context.Appointments.AnyAsync(a => a.DoctorId == userId);
                if (hasAppointments)
                {
                    return (false, "Cannot delete doctor with existing appointments");
                }
            }

            if (user.Role == UserRole.Patient)
            {
                var hasAppointments = await _context.Appointments.AnyAsync(a => a.PatientId == userId);
                if (hasAppointments)
                {
                    return (false, "Cannot delete patient with existing appointments");
                }
            }

            // Delete profile picture if exists
            if (!string.IsNullOrWhiteSpace(user.ProfilePicture))
            {
                DeleteProfilePictureFile(user.ProfilePicture);
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return (true, null);
        }

        public async Task<(bool Success, bool IsActive, string? ErrorMessage)> ToggleUserStatusAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, false, "User not found");
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            return (true, user.IsActive, null);
        }

        public async Task<(bool Success, string? ProfilePictureUrl, string? ErrorMessage)> UploadProfilePictureAsync(string userId, IFormFile file)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, null, "User not found");
            }

            // Validate file
            if (file == null || file.Length == 0)
            {
                return (false, null, "No file provided");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                return (false, null, "Invalid file format. Only jpg, jpeg, png, and gif are allowed");
            }

            if (file.Length > 5 * 1024 * 1024) // 5MB
            {
                return (false, null, "File size must not exceed 5MB");
            }

            // Delete old profile picture if exists
            if (!string.IsNullOrWhiteSpace(user.ProfilePicture))
            {
                DeleteProfilePictureFile(user.ProfilePicture);
            }

            // Save new file
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var profilePictureUrl = $"/uploads/profiles/{fileName}";
            user.ProfilePicture = profilePictureUrl;
            await _context.SaveChangesAsync();

            return (true, profilePictureUrl, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteProfilePictureAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            if (string.IsNullOrWhiteSpace(user.ProfilePicture))
            {
                return (false, "No profile picture to delete");
            }

            DeleteProfilePictureFile(user.ProfilePicture);
            user.ProfilePicture = null;
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<UserStatisticsDto> GetUserStatisticsAsync()
        {
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var statistics = new UserStatisticsDto
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalAdmins = await _context.Users.CountAsync(u => u.Role == UserRole.Admin),
                TotalDoctors = await _context.Users.CountAsync(u => u.Role == UserRole.Doctor),
                TotalReceptionists = await _context.Users.CountAsync(u => u.Role == UserRole.Receptionist),
                TotalPatients = await _context.Users.CountAsync(u => u.Role == UserRole.Patient),
                ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
                InactiveUsers = await _context.Users.CountAsync(u => !u.IsActive),
                NewUsersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= startOfMonth)
            };

            return statistics;
        }

        private void DeleteProfilePictureFile(string profilePictureUrl)
        {
            try
            {
                var fileName = Path.GetFileName(profilePictureUrl);
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "profiles", fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
                // Log error but don't fail the operation
            }
        }
    }
}

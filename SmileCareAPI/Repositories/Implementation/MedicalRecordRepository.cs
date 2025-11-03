using Microsoft.EntityFrameworkCore;
using SmileCareAPI.Data;
using SmileCareAPI.Models;
using SmileCareAPI.Repositories.Interface;

namespace SmileCareAPI.Repositories.Implementation
{
    public class MedicalRecordRepository : IMedicalRecordRepository
    {
        private readonly ApplicationDbContext _context;

        public MedicalRecordRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<MedicalRecord> Records, int TotalCount)> GetAllMedicalRecordsAsync(
            string? patientId,
            string? doctorId,
            DateTime? startDate,
            DateTime? endDate,
            int page,
            int pageSize)
        {
            var query = _context.MedicalRecords
                .Include(m => m.Patient)
                    .ThenInclude(p => p.User)
                .Include(m => m.Appointment)
                .AsQueryable();

            if (!string.IsNullOrEmpty(patientId))
            {
                query = query.Where(m => m.PatientId == patientId);
            }

            if (!string.IsNullOrEmpty(doctorId))
            {
                query = query.Where(m => m.Appointment.DoctorId == doctorId);
            }

            if (startDate.HasValue)
            {
                query = query.Where(m => m.VisitDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(m => m.VisitDate <= endDate.Value);
            }

            var totalCount = await query.CountAsync();

            var records = await query
                .OrderByDescending(m => m.VisitDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (records, totalCount);
        }

        public async Task<MedicalRecord?> GetMedicalRecordByIdAsync(int id)
        {
            return await _context.MedicalRecords
                .Include(m => m.Patient)
                    .ThenInclude(p => p.User)
                .Include(m => m.Appointment)
                .Include(m => m.Diagnoses)
                .Include(m => m.Treatments)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<(IEnumerable<MedicalRecord> Records, int TotalCount)> GetMedicalRecordsByPatientIdAsync(
            string patientId,
            int page,
            int pageSize)
        {
            var query = _context.MedicalRecords
                .Include(m => m.Patient)
                    .ThenInclude(p => p.User)
                .Include(m => m.Appointment)
                .Where(m => m.PatientId == patientId);

            var totalCount = await query.CountAsync();

            var records = await query
                .OrderByDescending(m => m.VisitDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (records, totalCount);
        }

        public async Task<MedicalRecord> CreateMedicalRecordAsync(MedicalRecord medicalRecord)
        {
            medicalRecord.CreatedAt = DateTime.UtcNow;
            _context.MedicalRecords.Add(medicalRecord);
            await _context.SaveChangesAsync();
            return medicalRecord;
        }

        public async Task<bool> UpdateMedicalRecordAsync(MedicalRecord medicalRecord)
        {
            medicalRecord.UpdatedAt = DateTime.UtcNow;
            _context.MedicalRecords.Update(medicalRecord);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteMedicalRecordAsync(int id)
        {
            var medicalRecord = await _context.MedicalRecords.FindAsync(id);
            if (medicalRecord == null)
                return false;

            _context.MedicalRecords.Remove(medicalRecord);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> MedicalRecordExistsAsync(int id)
        {
            return await _context.MedicalRecords.AnyAsync(m => m.Id == id);
        }

        public async Task<bool> PatientExistsAsync(string patientId)
        {
            return await _context.Patients.AnyAsync(p => p.UserId == patientId);
        }

        public async Task<bool> AppointmentExistsAsync(int appointmentId)
        {
            return await _context.Appointments.AnyAsync(a => a.Id == appointmentId);
        }

        public async Task<bool> IsAppointmentAlreadyLinkedAsync(int appointmentId)
        {
            return await _context.MedicalRecords.AnyAsync(m => m.AppointmentId == appointmentId);
        }
    }
}

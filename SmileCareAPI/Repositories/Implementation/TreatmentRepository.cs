using Microsoft.EntityFrameworkCore;
using SmileCareAPI.Data;
using SmileCareAPI.DTOs;
using SmileCareAPI.Models;
using SmileCareAPI.Repositories.Interface;

namespace SmileCareAPI.Repositories.Implementation
{
    public class TreatmentRepository : ITreatmentRepository
    {
        private readonly ApplicationDbContext _context;

        public TreatmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Treatment> treatments, int totalCount)> GetAllTreatmentsAsync(
            string? patientId,
            string? doctorId,
            TreatmentStatus? status,
            int page,
            int pageSize)
        {
            var query = _context.Treatments
                .Include(t => t.Patient)
                    .ThenInclude(p => p.User)
                .Include(t => t.Doctor)
                    .ThenInclude(d => d.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(patientId))
                query = query.Where(t => t.PatientId == patientId);

            if (!string.IsNullOrEmpty(doctorId))
                query = query.Where(t => t.DoctorId == doctorId);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            var totalCount = await query.CountAsync();

            var treatments = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (treatments, totalCount);
        }

        public async Task<Treatment?> GetTreatmentByIdAsync(int id)
        {
            return await _context.Treatments
                .Include(t => t.Patient)
                    .ThenInclude(p => p.User)
                .Include(t => t.Doctor)
                    .ThenInclude(d => d.User)
                .Include(t => t.Diagnosis)
                .Include(t => t.MedicalRecord)
                .Include(t => t.Sessions)
                .Include(t => t.Prescriptions)
                .Include(t => t.Images)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Treatment> CreateTreatmentAsync(Treatment treatment)
        {
            _context.Treatments.Add(treatment);
            await _context.SaveChangesAsync();
            return treatment;
        }

        public async Task<bool> UpdateTreatmentAsync(Treatment treatment)
        {
            treatment.UpdatedAt = DateTime.UtcNow;
            _context.Treatments.Update(treatment);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteTreatmentAsync(int id)
        {
            var treatment = await _context.Treatments.FindAsync(id);
            if (treatment == null)
                return false;

            _context.Treatments.Remove(treatment);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateTreatmentStatusAsync(int id, TreatmentStatus status)
        {
            var treatment = await _context.Treatments.FindAsync(id);
            if (treatment == null)
                return false;

            treatment.Status = status;
            treatment.UpdatedAt = DateTime.UtcNow;

            if (status == TreatmentStatus.Completed)
                treatment.ActualCompletionDate = DateTime.UtcNow;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<TreatmentSession> CreateTreatmentSessionAsync(TreatmentSession session)
        {
            _context.TreatmentSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<TreatmentSession?> GetTreatmentSessionByIdAsync(int treatmentId, int sessionId)
        {
            return await _context.TreatmentSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.TreatmentId == treatmentId);
        }

        public async Task<bool> CompleteTreatmentSessionAsync(int treatmentId, int sessionId, DateTime completedDate, string? notes)
        {
            var session = await GetTreatmentSessionByIdAsync(treatmentId, sessionId);
            if (session == null)
                return false;

            session.IsCompleted = true;
            session.CompletedDate = completedDate;
            if (!string.IsNullOrEmpty(notes))
                session.Notes = notes;

            // Update treatment completed sessions count
            var treatment = await _context.Treatments.FindAsync(treatmentId);
            if (treatment != null)
            {
                treatment.CompletedSessions = await GetCompletedSessionsCountAsync(treatmentId) + 1;
                treatment.UpdatedAt = DateTime.UtcNow;

                // Auto-complete treatment if all sessions are done
                if (treatment.CompletedSessions >= treatment.TotalSessions)
                {
                    treatment.Status = TreatmentStatus.Completed;
                    treatment.ActualCompletionDate = completedDate;
                }
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateTreatmentImagesAsync(int id, string? beforeImageUrl, string? afterImageUrl)
        {
            var treatment = await _context.Treatments.FindAsync(id);
            if (treatment == null)
                return false;

            if (!string.IsNullOrEmpty(beforeImageUrl))
                treatment.BeforeImageUrl = beforeImageUrl;

            if (!string.IsNullOrEmpty(afterImageUrl))
                treatment.AfterImageUrl = afterImageUrl;

            treatment.UpdatedAt = DateTime.UtcNow;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateTreatmentPaymentAsync(int id, decimal paidAmount, PaymentStatus? paymentStatus)
        {
            var treatment = await _context.Treatments.FindAsync(id);
            if (treatment == null)
                return false;

            treatment.PaidAmount = (treatment.PaidAmount ?? 0) + paidAmount;

            if (treatment.EstimatedCost.HasValue)
            {
                treatment.BalanceAmount = treatment.EstimatedCost.Value - treatment.PaidAmount.Value;
            }

            if (paymentStatus.HasValue)
                treatment.PaymentStatus = paymentStatus.Value;
            else
            {
                // Auto-determine payment status
                if (treatment.BalanceAmount <= 0)
                    treatment.PaymentStatus = PaymentStatus.Paid;
                else if (treatment.PaidAmount > 0)
                    treatment.PaymentStatus = PaymentStatus.Partial;
            }

            treatment.UpdatedAt = DateTime.UtcNow;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<TRTreatmentStatisticsDto> GetTreatmentStatisticsAsync(
            string? doctorId,
            DateTime? startDate,
            DateTime? endDate)
        {
            var query = _context.Treatments.AsQueryable();

            if (!string.IsNullOrEmpty(doctorId))
                query = query.Where(t => t.DoctorId == doctorId);

            if (startDate.HasValue)
                query = query.Where(t => t.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.CreatedAt <= endDate.Value);

            var totalTreatments = await query.CountAsync();
            var planned = await query.CountAsync(t => t.Status == TreatmentStatus.Planned);
            var ongoing = await query.CountAsync(t => t.Status == TreatmentStatus.Ongoing);
            var completed = await query.CountAsync(t => t.Status == TreatmentStatus.Completed);
            var onHold = await query.CountAsync(t => t.Status == TreatmentStatus.OnHold);
            var cancelled = await query.CountAsync(t => t.Status == TreatmentStatus.Cancelled);

            var completionRate = totalTreatments > 0
                ? Math.Round((decimal)completed / totalTreatments * 100, 2)
                : 0;

            var treatmentsByType = await query
                .GroupBy(t => t.TreatmentName)
                .Select(g => new TRTreatmentTypeCountDto
                {
                    TreatmentName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var totalRevenue = await query
                .Where(t => t.PaidAmount.HasValue)
                .SumAsync(t => t.PaidAmount.Value);

            var pendingPayments = await query
                .Where(t => t.BalanceAmount.HasValue && t.BalanceAmount > 0)
                .SumAsync(t => t.BalanceAmount.Value);

            return new TRTreatmentStatisticsDto
            {
                TotalTreatments = totalTreatments,
                Planned = planned,
                Ongoing = ongoing,
                Completed = completed,
                OnHold = onHold,
                Cancelled = cancelled,
                CompletionRate = completionRate,
                TreatmentsByType = treatmentsByType,
                TotalRevenue = totalRevenue,
                PendingPayments = pendingPayments
            };
        }

        public async Task<bool> TreatmentExistsAsync(int id)
        {
            return await _context.Treatments.AnyAsync(t => t.Id == id);
        }

        public async Task<bool> IsPatientOwnerAsync(int treatmentId, string patientId)
        {
            return await _context.Treatments
                .AnyAsync(t => t.Id == treatmentId && t.PatientId == patientId);
        }

        public async Task<bool> IsDoctorAssignedAsync(int treatmentId, string doctorId)
        {
            return await _context.Treatments
                .AnyAsync(t => t.Id == treatmentId && t.DoctorId == doctorId);
        }

        public async Task<int> GetCompletedSessionsCountAsync(int treatmentId)
        {
            return await _context.TreatmentSessions
                .CountAsync(s => s.TreatmentId == treatmentId && s.IsCompleted);
        }

        public async Task<decimal> GetTotalPaidAmountAsync(int treatmentId)
        {
            var treatment = await _context.Treatments.FindAsync(treatmentId);
            return treatment?.PaidAmount ?? 0;
        }
    }
}

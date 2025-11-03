using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmileCareAPI.Models;

namespace SmileCareAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet for each entity
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<OtpCode> OtpCodes { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<MedicalHistory> MedicalHistories { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Diagnosis> Diagnoses { get; set; }
        public DbSet<Treatment> Treatments { get; set; }
        public DbSet<AIResult> AIResults { get; set; }
        public DbSet<PatientImage> PatientImages { get; set; }
        public DbSet<SystemSettings> SystemSettings { get; set; }
        public DbSet<WorkingHours> WorkingHours { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<TreatmentSession> TreatmentSessions { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionMedication> PrescriptionMedications { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DentalChart> DentalCharts { get; set; }
        public DbSet<ClinicInformation> ClinicInformation { get; set; }
        public DbSet<Holiday> Holidays { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure composite keys and relationships if needed
            builder.Entity<Doctor>()
                .HasKey(d => d.UserId);

            builder.Entity<Patient>()
                .HasKey(p => p.UserId);

            builder.Entity<MedicalHistory>()
                .HasKey(mh => mh.PatientId);

            // Configure decimal precision for monetary and probability fields
            builder.Entity<Treatment>()
                .Property(t => t.EstimatedCost)
                .HasPrecision(10, 2);

            builder.Entity<Treatment>()
                .Property(t => t.PaidAmount)
                .HasPrecision(10, 2);

            builder.Entity<Treatment>()
                .Property(t => t.BalanceAmount)
                .HasPrecision(10, 2);

            builder.Entity<AIResult>()
                .Property(ai => ai.ConfidenceScore)
                .HasPrecision(5, 2);

            builder.Entity<AIResult>()
                .Property(ai => ai.HealthyProbability)
                .HasPrecision(5, 2);

            builder.Entity<AIResult>()
                .Property(ai => ai.CavityProbability)
                .HasPrecision(5, 2);

            builder.Entity<AIResult>()
                .Property(ai => ai.GumDiseaseProbability)
                .HasPrecision(5, 2);

            builder.Entity<Service>()
                .Property(s => s.DefaultPrice)
                .HasPrecision(10, 2);

            builder.Entity<TreatmentSession>()
                .Property(ts => ts.SessionCost)
                .HasPrecision(10, 2);
        }
    }
}

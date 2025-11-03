using System.ComponentModel.DataAnnotations;

namespace SmileCareAPI.Models
{
    public class ClinicInformation
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string ClinicName { get; set; }

        [MaxLength(300)]
        public string? Tagline { get; set; }

        public string? LogoUrl { get; set; }

        [MaxLength(100)]
        public string? LicenseNumber { get; set; }

        public DateTime? EstablishmentDate { get; set; }

        [MaxLength(100)]
        public string? ClinicType { get; set; }

        [MaxLength(20)]
        public string? PrimaryPhone { get; set; }

        [MaxLength(20)]
        public string? SecondaryPhone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string? Website { get; set; }

        [MaxLength(200)]
        public string? FacebookUrl { get; set; }

        [MaxLength(200)]
        public string? InstagramUrl { get; set; }

        [MaxLength(200)]
        public string? TwitterUrl { get; set; }

        [MaxLength(200)]
        public string? LinkedInUrl { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(300)]
        public string? StreetAddress { get; set; }

        [MaxLength(300)]
        public string? StreetAddress2 { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [MaxLength(500)]
        public string? Landmarks { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? Facilities { get; set; }

        [MaxLength(200)]
        public string? LanguagesSpoken { get; set; }

        [MaxLength(200)]
        public string? PaymentMethods { get; set; }

        [MaxLength(500)]
        public string? InsuranceCompanies { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
    public class Holiday
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public bool IsClosed { get; set; } = true;

        public TimeSpan? SpecialOpenTime { get; set; }

        public TimeSpan? SpecialCloseTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

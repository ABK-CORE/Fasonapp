using Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Entities.Dtos
{
    public class LoginDto:IDto
    {
        [JsonPropertyName("Username")]
        public string Username { get; set; }

        [JsonPropertyName("Password")]
        public string Password { get; set; }
    }

    public class RegisterDto : IDto
    {
        // --- Mevcut User Alanları ---
        [Required]
        [EmailAddress]
        [StringLength(75)]
        [JsonPropertyName("Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(1000)]
        [JsonPropertyName("Password")]
        public string Password { get; set; }

        [StringLength(75)]
        [JsonPropertyName("FirstName")]
        public string FirstName { get; set; }

        [StringLength(75)]
        [JsonPropertyName("LastName")]
        public string LastName { get; set; }

        [StringLength(75)]
        [JsonPropertyName("Username")]
        public string Username { get; set; }

        // --- Firma Bilgileri ---
        [Required]
        [StringLength(200)]
        [JsonPropertyName("CompanyName")]
        public string CompanyName { get; set; }

        [Required]
        [StringLength(100)]
        [JsonPropertyName("TaxOffice")]
        public string TaxOffice { get; set; }

        [Required]
        [StringLength(50)]
        [JsonPropertyName("TaxNumber")]
        public string TaxNumber { get; set; }

        [Required]
        [StringLength(50)]
        [JsonPropertyName("TradeRegisterNumber")]
        public string TradeRegisterNumber { get; set; }

        [Required]
        [StringLength(50)]
        [JsonPropertyName("CompanyType")]
        public string CompanyType { get; set; }

        [StringLength(50)]
        [JsonPropertyName("MerisNo")]
        public string? MerisNo { get; set; }

        // --- İletişim Bilgileri ---
        [Required]
        [StringLength(100)]
        [JsonPropertyName("ContactName")]
        public string ContactName { get; set; }

        [StringLength(100)]
        [JsonPropertyName("ContactPosition")]
        public string? ContactPosition { get; set; }

        [Required]
        [StringLength(50)]
        [JsonPropertyName("ContactPhoneNumber")]
        public string ContactPhoneNumber { get; set; }

        [Required]
        [StringLength(100)]
        [JsonPropertyName("ContactEmail")]
        public string ContactEmail { get; set; }

        [StringLength(200)]
        [JsonPropertyName("ContactWebsite")]
        public string? ContactWebsite { get; set; }

        [Required]
        [StringLength(250)]
        [JsonPropertyName("ContactAddress")]
        public string ContactAddress { get; set; }

        [StringLength(50)]
        [JsonPropertyName("PhoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("DateOfBirth")]
        public DateOnly? DateOfBirth { get; set; }

        [StringLength(350)]
        [JsonPropertyName("ProfilePictureUrl")]
        public string? ProfilePictureUrl { get; set; }

        [JsonPropertyName("Gender")]
        public bool? Gender { get; set; }

        [StringLength(250)]
        [JsonPropertyName("Address")]
        public string Address { get; set; }

        [StringLength(50)]
        [JsonPropertyName("Country")]
        public string Country { get; set; }

        [StringLength(10)]
        [JsonPropertyName("PreferredLanguage")]
        public string? PreferredLanguage { get; set; }

        [JsonPropertyName("TwoFactorEnabled")]
        public bool? TwoFactorEnabled { get; set; }
    }
}
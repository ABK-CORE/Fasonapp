using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class SupplierCreateDto : IDto
    {
        [Required, StringLength(75), EmailAddress]
        public string Email { get; set; }

        [Required, StringLength(1000)]
        public string Password { get; set; }

        [StringLength(75)]
        public string? FirstName { get; set; }

        [StringLength(75)]
        public string? LastName { get; set; }

        [StringLength(75)]
        public string? Username { get; set; }

        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? Country { get; set; }

        [Required, StringLength(200)]
        public string CompanyName { get; set; }
        [Required, StringLength(100)]
        public string TaxOffice { get; set; }
        [Required, StringLength(50)]
        public string TaxNumber { get; set; }
        [Required, StringLength(50)]
        public string TradeRegisterNumber { get; set; }
        [Required, StringLength(50)]
        public string CompanyType { get; set; }
        [StringLength(50)]
        public string? MerisNo { get; set; }

        [Required, StringLength(100)]
        public string ContactName { get; set; }
        [StringLength(100)]
        public string? ContactPosition { get; set; }
        [Required, StringLength(50)]
        public string ContactPhoneNumber { get; set; }
        [Required, StringLength(100), EmailAddress]
        public string ContactEmail { get; set; }
        [StringLength(200)]
        public string? ContactWebsite { get; set; }
        [Required, StringLength(250)]
        public string ContactAddress { get; set; }
    }

    public class SupplierUpdateDto : IDto
    {
        [Required]
        public Guid Guid { get; set; }

        [StringLength(75), EmailAddress]
        public string? Email { get; set; }

        [StringLength(75)]
        public string? FirstName { get; set; }

        [StringLength(75)]
        public string? LastName { get; set; }

        [StringLength(75)]
        public string? Username { get; set; }

        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? Country { get; set; }

        [StringLength(1000)]
        public string? Password { get; set; }

        [StringLength(200)] public string? CompanyName { get; set; }
        [StringLength(100)] public string? TaxOffice { get; set; }
        [StringLength(50)] public string? TaxNumber { get; set; }
        [StringLength(50)] public string? TradeRegisterNumber { get; set; }
        [StringLength(50)] public string? CompanyType { get; set; }
        [StringLength(50)] public string? MerisNo { get; set; }

        [StringLength(100)] public string? ContactName { get; set; }
        [StringLength(100)] public string? ContactPosition { get; set; }
        [StringLength(50)] public string? ContactPhoneNumber { get; set; }
        [StringLength(100), EmailAddress] public string? ContactEmail { get; set; }
        [StringLength(200)] public string? ContactWebsite { get; set; }
        [StringLength(250)] public string? ContactAddress { get; set; }
    }
    public class SupplierDto : IDto
    {
        public Guid Guid { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Country { get; set; }
        public bool? IsActive { get; set; }
        public string? CompanyName { get; set; }
        public string? TaxOffice { get; set; }
        public string? TaxNumber { get; set; }
        public string? TradeRegisterNumber { get; set; }
        public string? CompanyType { get; set; }
        public string? MerisNo { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPosition { get; set; }
        public string? ContactPhoneNumber { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactWebsite { get; set; }
        public string? ContactAddress { get; set; }
    }
}

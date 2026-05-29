using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Entities.Concrete.EntityFramework.Entities;

[Table("OPUser")]
[Index("Guid", Name = "UQ_User_Guid", IsUnique = true)]
public class User : IEntity {
    [Key]
    public int Id { get; set; }

    public Guid Guid { get; set; }

    [StringLength(75)]
    public string? Email { get; set; }

    [StringLength(1000)]
    public string? Password { get; set; }

    [StringLength(75)]
    public string? FirstName { get; set; }

    [StringLength(75)]
    public string? LastName { get; set; }

    [StringLength(75)]
    public string? Username { get; set; }

    [StringLength(50)]
    public string? PhoneNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastLoginDate { get; set; }

    public bool? IsEmailConfirmed { get; set; }

    public bool IsActive { get; set; }

    [StringLength(350)]
    public string? ProfilePictureUrl { get; set; }

    public bool? Gender { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    [StringLength(50)]
    public string? Country { get; set; }

    [StringLength(10)]
    public string? PreferredLanguage { get; set; }

    public bool? TwoFactorEnabled { get; set; }

    public string? EmailConfirmationCode { get; set; }
    public DateTime? EmailConfirmationExpireDate { get; set; }
    public bool EmailConfirmed { get; set; } = false;
    public string? PasswordResetCode { get; set; }
    public DateTime? PasswordResetExpireDate { get; set; }


    [InverseProperty("User")]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual SupplierFirmInfo? FirmInfo { get; set; }

    public virtual SupplierContactInfo? ContactInfo { get; set; }
    public virtual ICollection<Contract> Contracts { get; set; }
    [InverseProperty("User")]
    public virtual ICollection<DepartmentUser> DepartmentUsers { get; set; } = new List<DepartmentUser>();
    public virtual ICollection<RequestApprovalRecord> RequestApprovalRecords { get; set; } = new List<RequestApprovalRecord>();
    public virtual ICollection<UserPackage> UserPackages { get; set; } = new List<UserPackage>();

}

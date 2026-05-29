using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Entities.Concrete.EntityFramework.Entities;

[Table("OPRole")]
public class Role : IEntity {
    [Key]
    public int RoleId { get; set; }

    [StringLength(50)]
    public string RoleName { get; set; } = null!;

    [StringLength(250)]
    public string? Description { get; set; }

    [InverseProperty("Role")]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<PackageRole> PackageRoles { get; set; } = new List<PackageRole>();
}

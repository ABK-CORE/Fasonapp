using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Entities.Concrete.EntityFramework.Context;

public partial class ContextDb : DbContext
{
    public ContextDb()
    {
    }

    public ContextDb(DbContextOptions<ContextDb> options)
        : base(options)
    {
    }

    public virtual DbSet<ErrorLog> ErrorLogs { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserActivityLog> UserActivityLogs { get; set; }
    public virtual DbSet<UserRole> UserRoles { get; set; }
    public virtual DbSet<Part> Parts { get; set; }
    public virtual DbSet<ApprovalRule> ApprovalRules { get; set; }
    public virtual DbSet<ApprovalRuleApprover> ApprovalRuleApprovers { get; set; }
    public virtual DbSet<Tender> Tenders { get; set; }
    public virtual DbSet<TenderItem> TenderItems { get; set; }
    public virtual DbSet<ApprovalRecord> ApprovalRecords { get; set; }
    public virtual DbSet<PreApprovalApprover> PreApprovalApprovers { get; set; }
    public DbSet<TenderOffer> TenderOffers { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<SupplierFirmInfo> SupplierFirmInfos { get; set; }
    public DbSet<SupplierContactInfo> SupplierContactInfos { get; set; }
    public DbSet<PurchaseCategory> PurchaseCategories { get; set; } // Sınıf adıyla aynı olması best practice'dir.
    public DbSet<Department> Departments { get; set; }
    public DbSet<DepartmentUser> DepartmentUsers { get; set; }
    public DbSet<DepartmentApprovalProcess> DepartmentApprovalProcesses { get; set; }
    public DbSet<ApprovalProcessStep> ApprovalProcessSteps { get; set; }
    public DbSet<PurchaseRequest> PurchaseRequests { get; set; }
    public DbSet<RequestApprovalRecord> RequestApprovalRecords { get; set; }
    public DbSet<PurchaseRequestItem> PurchaseRequestItems { get; set; } // Sınıf adıyla aynı olması best practice'dir.
    public DbSet<RolePackage> RolePackages { get; set; }
    public DbSet<PackageRole> PackageRoles { get; set; }
    public DbSet<UserPackage> UserPackages { get; set; }
    public DbSet<usp_GetStoklarByStokKoduDto> usp_GetStoklarByStokKodu { get; set; }
    public DbSet<Entities.Contract> Contracts { get; set; } // DbSet olarak eklenmeli
    public DbSet<ContractFile> ContractFiles { get; set; } // DbSet olarak eklenmeli
    public DbSet<ContractPart> ContractParts { get; set; } // DbSet olarak eklenmeli

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Diğer tüm yapılandırmalarınız aynı kalır...
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("OPRole");
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE1A52B4D33F");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.UserRoleId).HasName("PK__UserRole__3D978A354708CBD1");
            entity.Property(e => e.AssignedDate).HasDefaultValueSql("(getdate())");
            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_UserRole_Role");
            entity.HasOne(d => d.User).WithMany(p => p.UserRoles).HasPrincipalKey(p => p.Guid).HasForeignKey(d => d.UserGuid).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_UserRole_UserGuid");
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.ToTable("OPPart");
            entity.HasKey(p => p.PartId);
            entity.Property(p => p.PartCode).IsRequired().HasMaxLength(50);
            entity.HasIndex(p => p.PartCode).IsUnique();
            entity.Property(p => p.PartName).IsRequired().HasMaxLength(250);
            entity.Property(p => p.Description).HasColumnType("nvarchar(max)").IsRequired(false);
        });

        modelBuilder.Entity<ApprovalRule>(entity =>
        {
            entity.HasKey(r => r.RuleId);
            entity.Property(r => r.MinAmount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(r => r.MaxAmount).HasColumnType("decimal(18,2)").IsRequired();
        });

        modelBuilder.Entity<ApprovalRuleApprover>(entity =>
        {
            entity.HasKey(a => a.RuleApproverId);
            entity.HasOne(a => a.Rule).WithMany(r => r.Approvers).HasForeignKey(a => a.RuleId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_ARA_Rule");
            entity.HasOne<User>().WithMany().HasForeignKey(a => a.UserGuid).HasPrincipalKey(u => u.Guid).OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_ARA_User");
            entity.Property(a => a.OrderIndex).IsRequired();
        });

        modelBuilder.Entity<Tender>(entity =>
        {
            entity.ToTable("OPTender");
            entity.HasKey(e => e.TenderId);
            entity.HasAlternateKey(e => e.TenderGuid).HasName("AK_Tender_TenderGuid");
            entity.Property(e => e.TenderGuid).IsRequired();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
            entity.Property(e => e.CreatedBy).IsRequired();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.HasMany(e => e.Items).WithOne(i => i.Tender).HasForeignKey(i => i.TenderGuid).HasPrincipalKey(e => e.TenderGuid).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.ApprovalRecords).WithOne(a => a.Tender).HasForeignKey(a => a.TenderGuid).HasPrincipalKey(e => e.TenderGuid).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TenderItem>(entity =>
        {
            entity.ToTable("OPTenderItem");
            entity.HasKey(e => e.TenderItemId);
            entity.Property(e => e.Quantity).IsRequired().HasColumnType("decimal(18,4)");
            entity.Property(e => e.Unit).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Note).HasColumnType("nvarchar(max)");
            entity.HasOne(e => e.Tender).WithMany(t => t.Items).HasForeignKey(e => e.TenderGuid).HasPrincipalKey(t => t.TenderGuid).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Part>().WithMany().HasForeignKey(e => e.PartId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("OPUser");
            entity.HasKey(e => e.Id);
            entity.HasAlternateKey(e => e.Guid).HasName("AK_User_Guid");
            entity.Property(e => e.Guid).IsRequired();
        });

        modelBuilder.Entity<ApprovalRecord>(entity =>
        {
            entity.ToTable("OPApprovalRecord");
            entity.HasKey(e => e.ApprovalId);
            entity.Property(e => e.OrderIndex).IsRequired();
            entity.Property(e => e.IsApproved);
            entity.Property(e => e.ActionDate);
            entity.HasIndex(e => new { e.TenderGuid, e.OrderIndex }).HasDatabaseName("IX_Approval_Tender");
            entity.HasOne(e => e.Tender).WithMany(t => t.ApprovalRecords).HasForeignKey(e => e.TenderGuid).HasPrincipalKey(t => t.TenderGuid).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<User>().WithMany().HasForeignKey(e => e.UserGuid).HasPrincipalKey(u => u.Guid).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PreApprovalApprover>(entity =>
        {
            entity.ToTable("OPPreApprovalApprover");
            entity.HasKey(e => e.PreApproverId);
            entity.Property(e => e.UserGuid).IsRequired();
            entity.Property(e => e.OrderIndex).IsRequired();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserGuid).HasPrincipalKey(u => u.Guid).OnDelete(DeleteBehavior.Cascade);
        });

        // ====================================================================
        // ===            TenderOffer İÇİN DÜZELTİLMİŞ BLOK                 ===
        // ====================================================================
        modelBuilder.Entity<TenderOffer>(entity =>
        {
            entity.ToTable("OPTenderOffer");
            entity.HasKey(e => e.OfferId);
            entity.HasAlternateKey(e => e.OfferGuid).HasName("AK_TenderOffer_OfferGuid");

            entity.Property(e => e.OfferGuid)
                  .IsRequired()
                  .ValueGeneratedOnAdd() // Bu daha doğru bir yaklaşım
                  .HasDefaultValueSql("NEWID()");

            // UnitPrice (eski Price) için doğru yapılandırma
            entity.Property(e => e.UnitPrice)
                  .HasColumnType("decimal(18,4)")
                  .IsRequired();

            // Yeni eklenen Currency için doğru yapılandırma
            entity.Property(e => e.Currency)
                  .HasMaxLength(10)
                  .IsRequired();

            // Yeni eklenen ExchangeRateToTL için doğru yapılandırma
            entity.Property(e => e.ExchangeRateToTL)
                  .HasColumnType("decimal(18,6)")
                  .IsRequired();

            // Yeni eklenen PriceInTL için doğru yapılandırma
            entity.Property(e => e.PriceInTL)
                  .HasColumnType("decimal(18,4)")
                  .IsRequired();

            // CreatedDate için doğru yapılandırma
            entity.Property(e => e.CreatedDate)
                  .HasColumnType("datetime2")
                  .HasDefaultValueSql("SYSUTCDATETIME()")
                  .IsRequired();

            // Diğer özellikler
            entity.Property(e => e.IsAccepted).IsRequired(false);
            entity.Property(e => e.SupplyDay).IsRequired();

            // İlişkiler
            entity.HasOne<Tender>()
                  .WithMany(t => t.Offers)
                  .HasForeignKey(e => e.TenderGuid)
                  .HasPrincipalKey(t => t.TenderGuid)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Part)
                  .WithMany()
                  .HasForeignKey(e => e.PartId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        // ====================================================================

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("OPAuditLog");
            entity.HasKey(e => e.LogId);
            entity.Property(e => e.LogId).ValueGeneratedOnAdd();
            entity.Property(e => e.Timestamp).HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
            entity.Property(e => e.UserGuid).IsRequired();
            entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.RequestId).IsRequired(false);
            entity.HasIndex(e => e.RequestId).HasDatabaseName("IX_AuditLog_RequestId");
        });

        modelBuilder.Entity<SupplierFirmInfo>()
            .HasOne(sf => sf.User)
            .WithOne(u => u.FirmInfo)
            .HasForeignKey<SupplierFirmInfo>(sf => sf.UserGuid)
            .HasPrincipalKey<User>(u => u.Guid)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SupplierContactInfo>()
            .HasOne(sc => sc.User)
            .WithOne(u => u.ContactInfo)
            .HasForeignKey<SupplierContactInfo>(sc => sc.UserGuid)
            .HasPrincipalKey<User>(u => u.Guid)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entities.Contract>(b =>
        {
            b.ToTable("OPContracts");
            b.HasKey(c => c.ContractId);
            b.HasIndex(c => c.ContractGuid).IsUnique();
            b.Property(c => c.ContractGuid).HasDefaultValueSql("NEWID()");
            b.Property(c => c.CreatedDate).HasDefaultValueSql("SYSUTCDATETIME()");
            b.HasOne(c => c.Supplier).WithMany(u => u.Contracts).HasForeignKey(c => c.SupplierGuid).HasPrincipalKey(u => u.Guid).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(c => c.CreatedByUser).WithMany().HasForeignKey(c => c.CreatedBy).HasPrincipalKey(u => u.Guid).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ContractFile>(b =>
        {
            b.ToTable("OPContractFiles");
            b.HasKey(f => f.FileId);
            b.Property(f => f.UploadedDate).HasDefaultValueSql("SYSUTCDATETIME()");
            b.HasOne(f => f.Contract).WithMany(c => c.Files).HasForeignKey(f => f.ContractId).OnDelete(DeleteBehavior.Cascade);
        });

        // ====================================================================
        // ===           ContractPart İÇİN DÜZELTİLMİŞ BLOK                 ===
        // ====================================================================
        modelBuilder.Entity<ContractPart>(b =>
        {
            b.ToTable("OPContractParts");
            b.HasKey(cp => new { cp.ContractId, cp.PartId });

            b.Property(cp => cp.UnitPrice)
             .HasColumnType("decimal(18,4)");

            // Yeni eklenen Currency sütununun yapılandırması
            b.Property(cp => cp.Currency)
             .HasMaxLength(10)
             .IsRequired()
             .HasDefaultValue("TL"); // Mevcut veriler için varsayılan değer

            b.HasOne(cp => cp.Contract)
             .WithMany(c => c.Parts)
             .HasForeignKey(cp => cp.ContractId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(cp => cp.Part)
             .WithMany()
             .HasForeignKey(cp => cp.PartId)
             .OnDelete(DeleteBehavior.Restrict);
        });
        // ====================================================================

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Department");
            entity.HasKey(d => d.DepartmentId);
            entity.HasIndex(d => d.Guid).IsUnique().HasDatabaseName("UQ_Department_Guid");
            entity.Property(d => d.Guid).IsRequired().HasDefaultValueSql("NEWID()");
            entity.Property(d => d.Name).IsRequired().HasMaxLength(250);
            entity.Property(d => d.Description).HasMaxLength(1000).IsRequired(false);
            entity.Property(d => d.CreatedDate).HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(d => d.IsActive).IsRequired().HasDefaultValue(true);
        });

        modelBuilder.Entity<DepartmentUser>(entity =>
        {
            entity.ToTable("DepartmentUser");
            entity.HasKey(du => du.DepartmentUserId);
            entity.Property(du => du.AssignedDate).HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(du => du.Department).WithMany(d => d.DepartmentUsers).HasForeignKey(du => du.DepartmentId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_DepartmentUser_Department");
            entity.HasOne(du => du.User).WithMany(u => u.DepartmentUsers).HasForeignKey(du => du.UserGuid).HasPrincipalKey(u => u.Guid).OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_DepartmentUser_User");
        });

        modelBuilder.Entity<DepartmentApprovalProcess>(b =>
        {
            b.ToTable("DepartmentApprovalProcess");
            b.HasKey(x => x.ProcessId);
            b.HasOne(x => x.Department).WithMany(d => d.ApprovalProcesses).HasForeignKey(x => x.DepartmentId);
        });

        modelBuilder.Entity<ApprovalProcessStep>(b =>
        {
            b.ToTable("ApprovalProcessStep");
            b.HasKey(x => x.StepId);
            b.HasOne(x => x.Process).WithMany(p => p.Steps).HasForeignKey(x => x.ProcessId);
            b.Property(x => x.StepType).IsRequired();
            b.Property(x => x.OrderIndex).IsRequired();
        });

        modelBuilder.Entity<RequestApprovalRecord>(b =>
        {
            b.ToTable("RequestApprovalRecord");
            b.HasKey(r => r.RecordId);
            b.HasOne(r => r.PurchaseRequest).WithMany(pr => pr.ApprovalRecords).HasForeignKey(r => r.RequestGuid).HasPrincipalKey(pr => pr.RequestGuid).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(r => r.User).WithMany(u => u.RequestApprovalRecords).HasForeignKey(r => r.UserGuid).HasPrincipalKey(u => u.Guid).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseRequest>(b =>
        {
            b.ToTable("PurchaseRequest");
            b.HasKey(pr => pr.RequestId);
            b.HasIndex(pr => pr.RequestGuid).IsUnique();
            b.Property(pr => pr.RequestGuid).IsRequired().HasDefaultValueSql("NEWID()");
            b.Property(pr => pr.Title).IsRequired().HasMaxLength(250);
            b.Property(pr => pr.Description).IsRequired(false);
            b.Property(pr => pr.RequiredDate).IsRequired();
            b.Property(pr => pr.Status).IsRequired();
            b.HasMany(pr => pr.Items).WithOne(i => i.PurchaseRequest).HasForeignKey(i => i.RequestGuid).HasPrincipalKey(pr => pr.RequestGuid).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(pr => pr.ApprovalRecords).WithOne(r => r.PurchaseRequest).HasForeignKey(r => r.RequestGuid).HasPrincipalKey(pr => pr.RequestGuid).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PurchaseRequestItem>(b =>
        {
            b.ToTable("PurchaseRequestItem");
            b.HasKey(i => i.ItemId);
            b.Property(i => i.Quantity).IsRequired();
            b.HasOne(i => i.PurchaseRequest).WithMany(pr => pr.Items).HasForeignKey(i => i.RequestGuid).HasPrincipalKey(pr => pr.RequestGuid).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(i => i.Part).WithMany().HasForeignKey(i => i.PartId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RolePackage>(entity =>
        {
            entity.ToTable("OPRolePackage");
            entity.HasKey(e => e.PackageId);
            entity.Property(e => e.PackageName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(250).IsRequired(false);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<PackageRole>(entity =>
        {
            entity.ToTable("OPPackageRole");
            entity.HasKey(e => e.PackageRoleId);
            entity.HasOne(e => e.Package).WithMany(p => p.PackageRoles).HasForeignKey(e => e.PackageId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Role).WithMany(r => r.PackageRoles).HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<UserPackage>(entity =>
        {
            entity.ToTable("OPUserPackage");
            entity.HasKey(e => e.UserPackageId);
            entity.Property(e => e.AssignedDate).HasDefaultValueSql("GETDATE()");
            entity.HasOne(e => e.User).WithMany(u => u.UserPackages).HasForeignKey(e => e.UserGuid).HasPrincipalKey(u => u.Guid).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Package).WithMany(p => p.UserPackages).HasForeignKey(e => e.PackageId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder
            .Entity<usp_GetStoklarByStokKoduDto>()
            .HasNoKey()
            .ToView(null);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
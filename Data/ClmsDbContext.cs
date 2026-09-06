using CLMS_APIs.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CLMS_APIs.Data;

public class ClmsDbContext : DbContext
{
    public ClmsDbContext(DbContextOptions<ClmsDbContext> options) : base(options)
    {
    }

    public DbSet<LoginEntity> Logins => Set<LoginEntity>();
    public DbSet<CompanyMaster> Companies => Set<CompanyMaster>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ContractorMaster> Contractors => Set<ContractorMaster>();
    public DbSet<OtherMaster> OtherMasters => Set<OtherMaster>();
    public DbSet<EmployeeMaster> EmployeeMasters => Set<EmployeeMaster>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LoginEntity>(entity =>
        {
            entity.ToTable("Login");
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Sid).HasPrecision(18, 0);
        });

        modelBuilder.Entity<CompanyMaster>(entity =>
        {
            entity.ToTable("CompanyMaster");
            entity.HasKey(e => e.CompanyId);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLog");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<ContractorMaster>(entity =>
        {
            entity.ToTable("ContractorMaster");
            entity.HasKey(e => e.ContractorId);
            entity.Property(e => e.ContractorId).HasPrecision(18, 0);
        });

        modelBuilder.Entity<OtherMaster>(entity =>
        {
            entity.ToTable("OtherMaster");
            entity.HasKey(e => e.MasterId);
        });

        modelBuilder.Entity<EmployeeMaster>(entity =>
        {
            entity.ToTable("EmployeeMaster");
            entity.HasKey(e => e.Sid);
            entity.Property(e => e.Sid).HasPrecision(18, 0);
        });
    }
}

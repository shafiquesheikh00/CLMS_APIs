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
    public DbSet<HolidayMaster> Holidays => Set<HolidayMaster>();
    public DbSet<ShiftMaster> Shifts => Set<ShiftMaster>();

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
            entity.HasKey(e => e.MasterTypeId);
            entity.Property(e => e.MasterTypeId).HasColumnName("MasterTypeID").ValueGeneratedOnAdd();
            entity.Property(e => e.MasterId).HasColumnName("MasterID").IsRequired();
            entity.Property(e => e.MasterName).HasColumnName("MasterName").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasColumnName("Description").HasMaxLength(200);
            entity.Property(e => e.Status).HasColumnName("Status");
            entity.Property(e => e.MasterType).HasColumnName("MasterType").HasMaxLength(150).IsRequired();
        });

        modelBuilder.Entity<EmployeeMaster>(entity =>
        {
            entity.ToTable("EmployeeMaster");
            entity.HasKey(e => e.Sid);
            entity.Property(e => e.Sid).HasPrecision(18, 0);
        });

        modelBuilder.Entity<HolidayMaster>(entity =>
        {
            entity.ToTable("HolidayMaster");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.HolidayDate).HasColumnName("HolidayDate").HasColumnType("datetime").IsRequired();
            entity.Property(e => e.Holiday_Desc).HasColumnName("Holiday_Desc").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Ispaid).HasColumnName("Ispaid").HasColumnType("bit").IsRequired();
        });

        modelBuilder.Entity<ShiftMaster>(entity =>
        {
            entity.ToTable("ShiftMaster");
            entity.HasKey(e => e.ShiftId);
            entity.Property(e => e.ShiftId).HasColumnName("ShiftID").HasPrecision(18, 0);
            entity.Property(e => e.ShiftName).HasColumnName("ShiftName").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Start_Time).HasColumnName("Start_Time").HasColumnType("datetime").IsRequired();
            entity.Property(e => e.End_Time).HasColumnName("End_Time").HasColumnType("datetime").IsRequired();
            entity.Property(e => e.Shift_Flag).HasColumnName("Shift_Flag").HasColumnType("bit");
            entity.Property(e => e.ShiftHours).HasColumnName("ShiftHours").HasPrecision(18, 2);
            entity.Property(e => e.GressTime).HasColumnName("GressTime");
        });
    }
}

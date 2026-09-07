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
    public DbSet<LabourRateMaster> LabourRateMasters => Set<LabourRateMaster>();

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

        modelBuilder.Entity<LabourRateMaster>(entity =>
        {
            entity.ToTable("LabourRateMaster");
            entity.HasKey(e => e.Rid);
            entity.Property(e => e.Rid).HasColumnName("RID").HasPrecision(18, 0);
            entity.Property(e => e.RdateFrom).HasColumnName("RdateFrom").HasColumnType("smalldatetime");
            entity.Property(e => e.Rdateto).HasColumnName("Rdateto").HasColumnType("smalldatetime");
            entity.Property(e => e.LabourCatId).HasColumnName("LabourCatID");
            entity.Property(e => e.RatePerDay).HasColumnName("RatePerDay").HasPrecision(18, 2);
            entity.Property(e => e.RateOTPerHour).HasColumnName("RateOTPerHour").HasPrecision(18, 2);
            entity.Property(e => e.Basic).HasColumnName("Basic").HasPrecision(18, 2);
            entity.Property(e => e.Special_Allowance).HasColumnName("Special_Allowance").HasPrecision(18, 2);
            entity.Property(e => e.Hra).HasColumnName("HRA").HasPrecision(18, 2);
            entity.Property(e => e.Other_Allowance).HasColumnName("Other_Allowance").HasPrecision(18, 2);
            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.LogDt).HasColumnName("LogDt").HasColumnType("smalldatetime");
            entity.Property(e => e.Hraper).HasColumnName("HRAPER").HasPrecision(18, 2);
            entity.Property(e => e.BonusPer).HasColumnName("BonusPER").HasPrecision(18, 2);
            entity.Property(e => e.Bonus).HasColumnName("Bonus").HasPrecision(18, 2);
            entity.Property(e => e.Lww).HasColumnName("LWW").HasPrecision(18, 2);
            entity.Property(e => e.Gross).HasColumnName("gross").HasPrecision(18, 2);
            entity.Property(e => e.Pfper).HasColumnName("PFPER").HasPrecision(18, 2);
            entity.Property(e => e.Pf).HasColumnName("PF").HasPrecision(18, 2);
            entity.Property(e => e.Attendance_Allow_App_After).HasColumnName("Attendance_Allow_App_After");
            entity.Property(e => e.Attendance_Allow_Rs).HasColumnName("Attendance_Allow_Rs").HasPrecision(18, 2);
            entity.Property(e => e.Da).HasColumnName("DA").HasPrecision(18, 2);
            entity.Property(e => e.Daper).HasColumnName("DAPER").HasPrecision(18, 2);
            entity.Property(e => e.P_F).HasColumnName("P_F").HasPrecision(18, 2);
            entity.Property(e => e.Esi).HasColumnName("ESI").HasPrecision(18, 2);
            entity.Property(e => e.Pt).HasColumnName("PT").HasPrecision(18, 2);
            entity.Property(e => e.Advance).HasColumnName("Advance").HasPrecision(18, 2);
            entity.Property(e => e.Lic).HasColumnName("LIC").HasPrecision(18, 2);
            entity.Property(e => e.Lwf).HasColumnName("LWF").HasPrecision(18, 2);
            entity.Property(e => e.EducationAllowance).HasColumnName("EducationAllowance").HasPrecision(18, 0);
            entity.Property(e => e.Other_All).HasColumnName("Other_All").HasPrecision(18, 0);
            entity.Property(e => e.Attendance_Allow_App_After2).HasColumnName("Attendance_Allow_App_After2");
            entity.Property(e => e.Attendance_Allow_Rs2).HasColumnName("Attendance_Allow_Rs2").HasPrecision(18, 2);
            entity.Property(e => e.Pfapply).HasColumnName("PFApply").HasMaxLength(50);
            entity.Property(e => e.Esicapply).HasColumnName("ESICApply").HasMaxLength(50);
            entity.Property(e => e.Ptapply).HasColumnName("PTApply").HasMaxLength(50);
            entity.Property(e => e.Attendance_Allow_Rs3).HasColumnName("Attendance_Allow_Rs3").HasPrecision(18, 2);
            entity.Property(e => e.Attendance_Allow_App_After3).HasColumnName("Attendance_Allow_App_After3");
            entity.Property(e => e.Stipend).HasColumnName("Stipend").HasPrecision(18, 2);
            entity.Property(e => e.ServiceCharge).HasColumnName("ServiceCharge").HasPrecision(18, 0);
            entity.Property(e => e.EmpCategoryFlag).HasColumnName("EmpCategoryFlag").HasMaxLength(10);
        });
    }
}

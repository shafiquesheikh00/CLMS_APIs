using System.Text;
using CLMS_APIs.Data;
using CLMS_APIs.Models.Entities;
using CLMS_APIs.Services;
using CLMS_APIs.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers
builder.Services.AddControllers();

// 2. Configure Database Context (Entity Framework Core SQL Server)
builder.Services.AddDbContext<ClmsDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.SqlServerEventId.DecimalTypeKeyWarning));
});

builder.Services.AddValidatorsFromAssemblyContaining<OtherMasterCreateDtoValidator>();

// 3. Register Application Services (Clean Architecture)
builder.Services.AddScoped<IPasswordVerifier, PlainTextPasswordVerifier>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IContractorService, ContractorService>();
builder.Services.AddScoped<IHolidayService, HolidayService>();
builder.Services.AddScoped<IOtherMasterService, OtherMasterService>();
builder.Services.AddScoped<IShiftService, ShiftService>();

// 4. Configure CORS for React Client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:58825",
                "https://localhost:58825")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// 5. Configure JWT Bearer Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"] 
             ?? jwtSettings["Key"] 
             ?? "CLMS_ContractorLabourManagementSystem_SecretKey_2026_MustBeAtLeast32BytesLong!";
var issuer = jwtSettings["Issuer"] ?? "CLMS_API";
var audience = jwtSettings["Audience"] ?? "CLMS_Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = authHeader.Substring("Bearer ".Length).Trim();
                }
                else if (!authHeader.Contains(' '))
                {
                    // If user pasted raw JWT token in Swagger without 'Bearer ' prefix
                    context.Token = authHeader.Trim();
                }
            }
            else if (builder.Environment.IsDevelopment())
            {
                // In Development mode: auto-fallback to a generated Dev Admin token so you don't need to paste anything when testing!
                var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();
                var devUser = new LoginEntity
                {
                    Uid = "1",
                    Username = "admin",
                    Role = "Admin"
                };
                context.Token = tokenService.GenerateToken(devUser);
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// 6. Configure Swagger with Bearer Authentication support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CLMS Web APIs",
        Version = "v1",
        Description = "Contractor Labour Management System API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token (you do not need to type 'Bearer ')."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Ensure AuditLog table exists in SQL Server
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ClmsDbContext>();
    try
    {
        dbContext.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLog')
            BEGIN
                CREATE TABLE AuditLog (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Module NVARCHAR(100) NOT NULL,
                    Action NVARCHAR(50) NOT NULL,
                    RecordId NVARCHAR(50) NOT NULL,
                    UserId NVARCHAR(50) NULL,
                    Timestamp DATETIME NOT NULL DEFAULT GETDATE()
                );
            END

            -- Ensure legacy optional columns on ContractorMaster exist
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ContractorMaster' AND COLUMN_NAME = 'SapNo')
                ALTER TABLE ContractorMaster ADD SapNo VARCHAR(50) NULL;
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ContractorMaster' AND COLUMN_NAME = 'PO_No')
                ALTER TABLE ContractorMaster ADD PO_No VARCHAR(50) NULL;
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ContractorMaster' AND COLUMN_NAME = 'PF_No')
                ALTER TABLE ContractorMaster ADD PF_No VARCHAR(50) NULL;
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ContractorMaster' AND COLUMN_NAME = 'PT_Code')
                ALTER TABLE ContractorMaster ADD PT_Code VARCHAR(50) NULL;
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ContractorMaster' AND COLUMN_NAME = 'ContactorType')
                ALTER TABLE ContractorMaster ADD ContactorType VARCHAR(50) NULL;
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ContractorMaster' AND COLUMN_NAME = 'PO_ValidFrom')
                ALTER TABLE ContractorMaster ADD PO_ValidFrom DATETIME NULL;
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ContractorMaster' AND COLUMN_NAME = 'PO_ValidTo')
                ALTER TABLE ContractorMaster ADD PO_ValidTo DATETIME NULL;

            -- Ensure HolidayMaster table exists
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'HolidayMaster')
            BEGIN
                CREATE TABLE HolidayMaster (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    HolidayDate DATETIME NOT NULL,
                    Holiday_Desc NVARCHAR(50) NOT NULL,
                    Ispaid BIT NOT NULL
                );
            END

            -- Ensure OtherMaster table exists
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OtherMaster')
            BEGIN
                CREATE TABLE OtherMaster (
                    MasterTypeID INT IDENTITY(1,1) PRIMARY KEY,
                    MasterID INT NOT NULL,
                    MasterName VARCHAR(50) NOT NULL,
                    Description VARCHAR(200) NULL,
                    Status BIT NOT NULL DEFAULT 1,
                    MasterType NVARCHAR(150) NOT NULL
                );
            END

            -- Ensure ShiftMaster table exists
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ShiftMaster')
            BEGIN
                CREATE TABLE ShiftMaster (
                    ShiftID NUMERIC(18,0) PRIMARY KEY,
                    ShiftName VARCHAR(50) NOT NULL,
                    Start_Time DATETIME NOT NULL,
                    End_Time DATETIME NOT NULL,
                    Shift_Flag BIT NULL DEFAULT 0,
                    ShiftHours DECIMAL(18,2) NULL,
                    GressTime FLOAT NULL DEFAULT 0
                );
            END
");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not verify/create AuditLog table automatically.");
    }
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CLMS Web APIs v1");
    c.EnablePersistAuthorization();
});

// Redirect root to swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"));

// Ensure wwwroot directory exists
var webRoot = app.Environment.WebRootPath;
if (string.IsNullOrWhiteSpace(webRoot))
{
    webRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
}
Directory.CreateDirectory(webRoot);
Directory.CreateDirectory(Path.Combine(webRoot, "uploads", "company-logos"));

// Serve static files (including company logos from wwwroot)
app.UseStaticFiles();

// Use CORS before Authentication and Authorization
app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

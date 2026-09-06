using CLMS_APIs.Data;
using CLMS_APIs.Models.Entities;

namespace CLMS_APIs.Services;

public class AuditLogService : IAuditLogService
{
    private readonly ClmsDbContext _context;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(ClmsDbContext context, ILogger<AuditLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogAsync(string module, string action, string recordId, string? userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditEntry = new AuditLog
            {
                Module = module,
                Action = action,
                RecordId = recordId,
                UserId = userId,
                Timestamp = DateTime.Now
            };

            _context.AuditLogs.Add(auditEntry);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("AuditLog: [{Module}] {Action} on Record {RecordId} by User {UserId}", module, action, recordId, userId ?? "Anonymous");
        }
        catch (Exception ex)
        {
            // Do not let audit logging failure crash the primary business operation, but log server-side
            _logger.LogError(ex, "Failed to persist audit log for Module: {Module}, Action: {Action}, RecordId: {RecordId}", module, action, recordId);
        }
    }
}

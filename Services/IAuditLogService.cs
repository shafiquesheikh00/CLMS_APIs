namespace CLMS_APIs.Services;

public interface IAuditLogService
{
    Task LogAsync(string module, string action, string recordId, string? userId, CancellationToken cancellationToken = default);
}

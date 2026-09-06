namespace CLMS_APIs.Exceptions;

public class ContractorConflictException : Exception
{
    public string ConflictField { get; }

    public ContractorConflictException(string conflictField, string message) : base(message)
    {
        ConflictField = conflictField;
    }
}

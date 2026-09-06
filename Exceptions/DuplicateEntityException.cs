namespace CLMS_APIs.Exceptions;

public class DuplicateEntityException : Exception
{
    public DuplicateEntityException(string message) : base(message)
    {
    }
}

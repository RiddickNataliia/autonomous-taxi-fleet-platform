namespace NovaDrive.Domain.Exceptions;

public class MaintenanceDomainException : Exception
{
    public MaintenanceDomainException(string message) : base(message)
    {
    }
}
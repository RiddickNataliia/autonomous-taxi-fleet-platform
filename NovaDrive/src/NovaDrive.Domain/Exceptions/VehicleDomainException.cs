namespace NovaDrive.Domain.Exceptions;

public class VehicleDomainException : Exception
{
    public VehicleDomainException(string message) : base(message)
    {
    }
}
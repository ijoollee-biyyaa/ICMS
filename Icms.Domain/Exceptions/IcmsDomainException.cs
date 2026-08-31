namespace Icms.Domain.Exceptions;

public class IcmsDomainException : Exception
{
    public IcmsDomainException(string message) : base(message)
    {
    }
}

namespace Icms.Application.Exceptions;

public class UniqueConstraintViolationException : Exception
{
    public string? ConstraintName { get; }

    public UniqueConstraintViolationException(
        string message, Exception innerException, string? constraintName = null)
        : base(message, innerException)
    {
        ConstraintName = constraintName;
    }
}
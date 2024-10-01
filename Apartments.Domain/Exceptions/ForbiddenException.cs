namespace Apartments.Domain.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException() : base("Operation forbidden")
    {
    }

    public ForbiddenException(string message) : base(message)
    {
    }
}
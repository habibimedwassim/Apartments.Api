namespace Apartments.Domain.Exceptions;

public class AppValidationException : Exception
{
    public List<string> Errors { get; }

    public AppValidationException(List<string> errors)
        : base("Validation failed.")
    {
        Errors = errors;
    }

    public AppValidationException(string message, List<string> errors)
        : base(message)
    {
        Errors = errors;
    }

    public AppValidationException(string message, Exception innerException, List<string> errors)
        : base(message, innerException)
    {
        Errors = errors;
    }
}
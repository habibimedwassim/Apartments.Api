namespace Apartments.Domain.Exceptions;

public class AppValidationException(List<string> errors) : Exception("Validation failed.")
{
    public List<string> Errors { get; } = errors;
}
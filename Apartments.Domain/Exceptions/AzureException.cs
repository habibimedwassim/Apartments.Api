namespace Apartments.Domain.Exceptions;

public class AzureException(string message) : Exception(message)
{
}
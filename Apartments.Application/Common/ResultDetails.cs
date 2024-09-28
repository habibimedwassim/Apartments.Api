namespace Apartments.Application.Common;

public class ResultDetails
{
    public string Message { get; }
    public IEnumerable<string> Errors { get; }

    public ResultDetails(string? message, IEnumerable<string>? errors = null)
    {
        Message = message ?? string.Empty;
        Errors = errors ?? Enumerable.Empty<string>();
    }
}
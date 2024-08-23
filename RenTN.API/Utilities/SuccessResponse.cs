namespace RenTN.API.Utilities;

public class SuccessResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}
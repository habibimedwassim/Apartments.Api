namespace RenTN.API.Utilities;
public class ErrorResponse
{
    public int Status { get; set; }
    public string Error { get; set; } = default!;
}
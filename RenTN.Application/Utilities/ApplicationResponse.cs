namespace RenTN.Application.Utilities;

public class ApplicationResponse
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public object? ResponseModel { get; set; }

    public ApplicationResponse(bool success, int statusCode, string message, object? responseModel = null)
    {
        Success = success;
        StatusCode = statusCode;
        Message = message;
        ResponseModel = responseModel;
    }
}

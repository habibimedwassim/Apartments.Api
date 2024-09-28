using Microsoft.AspNetCore.Http;

namespace Apartments.Application.Common;

public class ServiceResult<T>
{
    public bool Success { get; }
    public int StatusCode { get; }
    public string? Message { get; }
    public T? Data { get; }

    private ServiceResult(bool success, int statusCode, string? message, T? data)
    {
        Success = success;
        StatusCode = statusCode;
        Message = message;
        Data = data;
    }

    public static ServiceResult<T> SuccessResult(T data, string message = "Operation successful.")
    {
        return new ServiceResult<T>(true, StatusCodes.Status200OK, message, data);
    }

    public static ServiceResult<T> ErrorResult(int statusCode, string message)
    {
        return new ServiceResult<T>(false, statusCode, message, default);
    }

    public static ServiceResult<T> InfoResult(int statusCode, string message)
    {
        return new ServiceResult<T>(true, statusCode, message, default);
    }
}
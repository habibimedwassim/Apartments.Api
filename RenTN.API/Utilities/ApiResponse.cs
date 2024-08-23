using Microsoft.AspNetCore.Mvc;
using RenTN.Application.Utilities;

namespace RenTN.API.Utilities;

public static class ApiResponse
{
    public static IActionResult Error(ApplicationResponse response)
    {
        var errorResponse = new ErrorResponse
        {
            Status = response.StatusCode,
            Error = response.Message
        };
        return new ObjectResult(errorResponse) { StatusCode = response.StatusCode };
    }
    public static IActionResult Success(ApplicationResponse response)
    {
        var successResponse = new SuccessResponse
        {
            Status = response.StatusCode,
            Message = response.Message,
            Data = response.ResponseModel
        };

        if (successResponse.Data == null)
        {
            return new ObjectResult(new { successResponse.Status, successResponse.Message }) { StatusCode = successResponse.Status };
        }

        return new ObjectResult(successResponse) { StatusCode = successResponse.Status };
    }
}
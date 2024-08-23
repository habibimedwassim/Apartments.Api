using Microsoft.AspNetCore.Mvc;
using RenTN.API.Utilities;
using RenTN.Application.Utilities;
using RenTN.Domain.Exceptions;

namespace RenTN.API.Middlewares;

public class ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> _logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next.Invoke(context);
        }
        catch(UnauthorizedAccessException ex)
        {
            _logger.LogError(ex.Message);
            await HandleExceptionAsync(context, StatusCodes.Status401Unauthorized, ex.Message);
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex.Message);
            await HandleExceptionAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (ForbiddenException)
        {
            _logger.LogError("Operation forbidden");
            await HandleExceptionAsync(context, StatusCodes.Status403Forbidden, "Operation forbidden");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, StatusCodes.Status500InternalServerError, "Something went wrong!");
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, int statusCode, string errorMessage)
    {
        var applicationResponse = new ApplicationResponse(false, statusCode, errorMessage);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var errorResult = ApiResponse.Error(applicationResponse) as ObjectResult;

        return context.Response.WriteAsJsonAsync(new
        {
            errorResult?.StatusCode,
            errorResult?.Value
        });
    }
}

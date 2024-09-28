using Apartments.Application.Common;
using Apartments.Domain.Exceptions;
using System.Text.Json;

namespace Apartments.API.Middlewares;

public class ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next.Invoke(context);
        }
        catch (BadRequestException ex)
        {
            logger.LogWarning(ex, ex.Message);
            await HandleExceptionAsync(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (AppValidationException ex)
        {
            logger.LogWarning(ex, "Validation error occurred.");
            var resultDetails = new ResultDetails(ex.Message, ex.Errors);

            await HandleExceptionAsync(context, StatusCodes.Status400BadRequest, resultDetails);
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning(ex, ex.Message);
            await HandleExceptionAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (ForbiddenException ex)
        {
            logger.LogWarning(ex, ex.Message);
            await HandleExceptionAsync(context, StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (AzureException ex)
        {
            logger.LogWarning(ex.Message);
            await HandleExceptionAsync(context, StatusCodes.Status424FailedDependency, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, StatusCodes.Status500InternalServerError, "Something went wrong, please contact the Admin!");
        }
    }
    private static Task HandleExceptionAsync(HttpContext context, int statusCode, string message)
    {
        var resultDetails = new ResultDetails(message);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        var result = JsonSerializer.Serialize(resultDetails);
        return context.Response.WriteAsync(result);
    }
    private static Task HandleExceptionAsync(HttpContext context, int statusCode, object resultDetails)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        var result = JsonSerializer.Serialize(resultDetails);
        return context.Response.WriteAsync(result);
    }
}

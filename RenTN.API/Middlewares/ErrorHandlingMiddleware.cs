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
        catch (NotFoundException ex)
        {
            _logger.LogError(ex.Message);

            context.Response.StatusCode = 404;
            await context.Response.WriteAsync(ex.Message);
        }
        catch (ForbiddenException)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Operation forbidden");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Something went wrong!");
        }
    }
}


using Apartments.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Apartments.API.Extensions;

public class ValidationFilter(IServiceProvider serviceProvider) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null) continue;

            var validator = serviceProvider.GetService(typeof(IValidator<>).MakeGenericType(argument.GetType())) as IValidator;

            if (validator != null)
            {
                var validationContext = new ValidationContext<object>(argument);

                var validationResult = validator.Validate(validationContext);

                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                        .ToList();

                    throw new AppValidationException(errors);
                }
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
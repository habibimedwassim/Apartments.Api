using Apartments.Application.Dtos.UserDtos;
using Apartments.Application.Utilities;
using FluentValidation;

namespace Apartments.Application.Validators;

//public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
//{
//    public UpdateUserValidator()
//    {
//        RuleFor(x => x.PhoneNumber)
//            .Must(CoreUtilities.ValidatePhoneNumber)
//            .WithMessage("Please provide a valid phone number.")
//            .When(x => x.PhoneNumber != null);
//    }
//}

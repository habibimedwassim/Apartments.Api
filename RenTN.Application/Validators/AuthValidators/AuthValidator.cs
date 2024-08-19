using FluentValidation;
using RenTN.Application.DTOs.AuthDTOs;

namespace RenTN.Application.Validators.AuthValidators;

public class EmailValidator : AbstractValidator<EmailDTO>
{
    public EmailValidator()
    {
        RuleFor(x => x.Email)
           .EmailAddress()
           .WithMessage("Invalid Email");
    }
}
public class LoginValidator : AbstractValidator<LoginDTO>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
           .EmailAddress()
           .WithMessage("Invalid Email");
    }
}
public class RegisterValidator : AbstractValidator<RegisterDTO>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
           .EmailAddress()
           .WithMessage("Invalid Email");

        RuleFor(x => x.PhoneNumber).Matches(@"^\d{8}").WithMessage("Please provide a valid phone number (8 digits).")
                                   .When(x => x.PhoneNumber != null);
    }
}

public class ResetPasswordValidator : AbstractValidator<ResetPasswordDTO>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Email)
           .EmailAddress()
           .WithMessage("Invalid Email");
    }
}

public class VerifyEmailValidator : AbstractValidator<VerifyEmailDTO>
{
    public VerifyEmailValidator()
    {
        RuleFor(x => x.Email)
           .EmailAddress()
           .WithMessage("Invalid Email");
    }
}
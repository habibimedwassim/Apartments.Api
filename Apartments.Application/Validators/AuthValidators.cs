using Apartments.Application.Dtos.AuthDtos;
using FluentValidation;

namespace Apartments.Application.Validators;

public class EmailValidator : AbstractValidator<EmailDto>
{
    public EmailValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Invalid Email");
    }
}
public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Invalid Email");
    }
}
public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
           .EmailAddress()
           .WithMessage("Invalid Email");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\d{8}")
            .WithMessage("Please provide a valid phone number (8 digits).")
            .When(x => x.PhoneNumber != null);

        RuleFor(x => x.Gender)
            .Must(x => x == "Male" || x == "Female")
            .WithMessage("Please enter a valid Gender (Male or Female)");

        RuleFor(x => x.DateOfBirth)
            .Must(BeAtLeast18YearsOld)
            .When(x => x.DateOfBirth != null)
            .WithMessage("You must be at least 18 years old.");
    }
    private bool BeAtLeast18YearsOld(DateOnly? dateOfBirth)
    {
        if (dateOfBirth == null) return false;

        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - dateOfBirth.Value.Year;

        if (dateOfBirth.Value.AddYears(age) > today)
        {
            age--;
        }

        return age >= 18;
    }
}

public class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Invalid Email");

        RuleFor(x => x.VerificationCode)
            .Matches(@"^\d{4}")
            .WithMessage("Please provide a valid verification code (4 digits).");
    }
}

public class VerifyEmailValidator : AbstractValidator<VerifyEmailDto>
{
    public VerifyEmailValidator()
    {
        RuleFor(x => x.Email)
           .EmailAddress()
           .WithMessage("Invalid Email");

        RuleFor(x => x.VerificationCode)
            .Matches(@"^\d{4}")
            .WithMessage("Please provide a valid verification code (4 digits).");
    }
}
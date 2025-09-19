using FluentValidation;

namespace Generated.DTOs.Validators;

/// <summary>
/// FluentValidation validator for Error
/// </summary>
public class ErrorValidator : AbstractValidator<Error>
{
    public ErrorValidator()
    {
        RuleFor(x =&gt; x.Code).NotEmpty();
        RuleFor(x =&gt; x.Message).NotEmpty();
    }
}
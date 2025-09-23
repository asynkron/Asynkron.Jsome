using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16GetLogResponse
/// </summary>
public class V16GetLogResponseValidator : AbstractValidator<V16GetLogResponse>
{
    public V16GetLogResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.Filename)
            .MaximumLength(255).WithMessage("Must be no more than 255 characters long")
            ;
    }
}
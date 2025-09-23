using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16ResetResponse
/// </summary>
public class V16ResetResponseValidator : AbstractValidator<V16ResetResponse>
{
    public V16ResetResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "Accepted", "Rejected" }.Contains(x.ToString())).WithMessage("Must be one of: "Accepted", "Rejected"")
            ;
    }
}
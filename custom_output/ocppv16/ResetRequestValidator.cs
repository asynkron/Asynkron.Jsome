using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16ResetRequest
/// </summary>
public class V16ResetRequestValidator : AbstractValidator<V16ResetRequest>
{
    public V16ResetRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "Hard", "Soft" }.Contains(x.ToString())).WithMessage("Must be one of: "Hard", "Soft"")
            ;
    }
}
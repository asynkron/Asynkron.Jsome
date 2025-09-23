using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16TriggerMessageResponse
/// </summary>
public class V16TriggerMessageResponseValidator : AbstractValidator<V16TriggerMessageResponse>
{
    public V16TriggerMessageResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "Accepted", "Rejected", "NotImplemented" }.Contains(x.ToString())).WithMessage("Must be one of: "Accepted", "Rejected", "NotImplemented"")
            ;
    }
}
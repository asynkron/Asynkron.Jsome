using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16UnlockConnectorResponse
/// </summary>
public class V16UnlockConnectorResponseValidator : AbstractValidator<V16UnlockConnectorResponse>
{
    public V16UnlockConnectorResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "Unlocked", "UnlockFailed", "NotSupported" }.Contains(x.ToString())).WithMessage("Must be one of: "Unlocked", "UnlockFailed", "NotSupported"")
            ;
    }
}
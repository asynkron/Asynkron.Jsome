using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16ClearChargingProfileResponse
/// </summary>
public class V16ClearChargingProfileResponseValidator : AbstractValidator<V16ClearChargingProfileResponse>
{
    public V16ClearChargingProfileResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "Accepted", "Unknown" }.Contains(x.ToString())).WithMessage("Must be one of: "Accepted", "Unknown"")
            ;
    }
}
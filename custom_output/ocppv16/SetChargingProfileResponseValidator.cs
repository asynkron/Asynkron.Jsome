using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16SetChargingProfileResponse
/// </summary>
public class V16SetChargingProfileResponseValidator : AbstractValidator<V16SetChargingProfileResponse>
{
    public V16SetChargingProfileResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "Accepted", "Rejected", "NotSupported" }.Contains(x.ToString())).WithMessage("Must be one of: "Accepted", "Rejected", "NotSupported"")
            ;
    }
}
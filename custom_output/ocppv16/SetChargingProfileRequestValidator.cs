using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16SetChargingProfileRequest
/// </summary>
public class V16SetChargingProfileRequestValidator : AbstractValidator<V16SetChargingProfileRequest>
{
    public V16SetChargingProfileRequestValidator()
    {
        RuleFor(x => x.ConnectorId)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.CsChargingProfiles)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
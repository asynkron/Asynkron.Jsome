using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16ClearChargingProfileRequest
/// </summary>
public class V16ClearChargingProfileRequestValidator : AbstractValidator<V16ClearChargingProfileRequest>
{
    public V16ClearChargingProfileRequestValidator()
    {
        RuleFor(x => x.ChargingProfilePurpose)
            .Must(x => new[] { "ChargePointMaxProfile", "TxDefaultProfile", "TxProfile" }.Contains(x.ToString())).WithMessage("Must be one of: "ChargePointMaxProfile", "TxDefaultProfile", "TxProfile"")
            ;
    }
}
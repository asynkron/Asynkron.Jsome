using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16SignedUpdateFirmware
/// </summary>
public class V16SignedUpdateFirmwareValidator : AbstractValidator<V16SignedUpdateFirmware>
{
    public V16SignedUpdateFirmwareValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.Firmware)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16BootNotificationRequest
/// </summary>
public class V16BootNotificationRequestValidator : AbstractValidator<V16BootNotificationRequest>
{
    public V16BootNotificationRequestValidator()
    {
        RuleFor(x => x.ChargePointVendor)
            .NotEmpty.WithMessage("This field is required")
            .MaximumLength(20).WithMessage("Must be no more than 20 characters long")
            ;
        RuleFor(x => x.ChargePointModel)
            .NotEmpty.WithMessage("This field is required")
            .MaximumLength(20).WithMessage("Must be no more than 20 characters long")
            ;
        RuleFor(x => x.ChargePointSerialNumber)
            .MaximumLength(25).WithMessage("Must be no more than 25 characters long")
            ;
        RuleFor(x => x.ChargeBoxSerialNumber)
            .MaximumLength(25).WithMessage("Must be no more than 25 characters long")
            ;
        RuleFor(x => x.FirmwareVersion)
            .MaximumLength(50).WithMessage("Must be no more than 50 characters long")
            ;
        RuleFor(x => x.Iccid)
            .MaximumLength(20).WithMessage("Must be no more than 20 characters long")
            ;
        RuleFor(x => x.Imsi)
            .MaximumLength(20).WithMessage("Must be no more than 20 characters long")
            ;
        RuleFor(x => x.MeterType)
            .MaximumLength(25).WithMessage("Must be no more than 25 characters long")
            ;
        RuleFor(x => x.MeterSerialNumber)
            .MaximumLength(25).WithMessage("Must be no more than 25 characters long")
            ;
    }
}
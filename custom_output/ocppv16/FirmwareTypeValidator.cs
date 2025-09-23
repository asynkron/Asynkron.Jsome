using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16FirmwareType
/// </summary>
public class V16FirmwareTypeValidator : AbstractValidator<V16FirmwareType>
{
    public V16FirmwareTypeValidator()
    {
        RuleFor(x => x.Location)
            .NotEmpty.WithMessage("This field is required")
            .MaximumLength(512).WithMessage("Must be no more than 512 characters long")
            ;
        RuleFor(x => x.RetrieveDateTime)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.SigningCertificate)
            .NotEmpty.WithMessage("This field is required")
            .MaximumLength(5500).WithMessage("Must be no more than 5500 characters long")
            ;
        RuleFor(x => x.Signature)
            .NotEmpty.WithMessage("This field is required")
            .MaximumLength(800).WithMessage("Must be no more than 800 characters long")
            ;
    }
}
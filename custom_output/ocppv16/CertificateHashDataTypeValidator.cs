using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16CertificateHashDataType
/// </summary>
public class V16CertificateHashDataTypeValidator : AbstractValidator<V16CertificateHashDataType>
{
    public V16CertificateHashDataTypeValidator()
    {
        RuleFor(x => x.HashAlgorithm)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.IssuerNameHash)
            .NotEmpty.WithMessage("This field is required")
            .MaximumLength(128).WithMessage("Must be no more than 128 characters long")
            ;
        RuleFor(x => x.IssuerKeyHash)
            .NotEmpty.WithMessage("This field is required")
            .MaximumLength(128).WithMessage("Must be no more than 128 characters long")
            ;
        RuleFor(x => x.SerialNumber)
            .NotEmpty.WithMessage("This field is required")
            .MaximumLength(40).WithMessage("Must be no more than 40 characters long")
            ;
    }
}
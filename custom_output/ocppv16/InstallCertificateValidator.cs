using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16InstallCertificate
/// </summary>
public class V16InstallCertificateValidator : AbstractValidator<V16InstallCertificate>
{
    public V16InstallCertificateValidator()
    {
        RuleFor(x => x.CertificateType)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.Certificate)
            .NotEmpty.WithMessage("This field is required")
            .MaximumLength(5500).WithMessage("Must be no more than 5500 characters long")
            ;
    }
}
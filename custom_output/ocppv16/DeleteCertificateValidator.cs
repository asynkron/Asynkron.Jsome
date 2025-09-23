using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16DeleteCertificate
/// </summary>
public class V16DeleteCertificateValidator : AbstractValidator<V16DeleteCertificate>
{
    public V16DeleteCertificateValidator()
    {
        RuleFor(x => x.CertificateHashData)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
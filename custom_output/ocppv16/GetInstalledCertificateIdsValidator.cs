using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16GetInstalledCertificateIds
/// </summary>
public class V16GetInstalledCertificateIdsValidator : AbstractValidator<V16GetInstalledCertificateIds>
{
    public V16GetInstalledCertificateIdsValidator()
    {
        RuleFor(x => x.CertificateType)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
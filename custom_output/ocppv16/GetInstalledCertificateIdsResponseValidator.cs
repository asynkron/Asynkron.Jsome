using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16GetInstalledCertificateIdsResponse
/// </summary>
public class V16GetInstalledCertificateIdsResponseValidator : AbstractValidator<V16GetInstalledCertificateIdsResponse>
{
    public V16GetInstalledCertificateIdsResponseValidator()
    {
        RuleFor(x => x.CertificateHashData)
            .Must(x => x.Count >= 1).WithMessage("Must contain at least 1 items")
            ;
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
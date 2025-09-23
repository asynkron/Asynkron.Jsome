using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16InstallCertificateResponse
/// </summary>
public class V16InstallCertificateResponseValidator : AbstractValidator<V16InstallCertificateResponse>
{
    public V16InstallCertificateResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
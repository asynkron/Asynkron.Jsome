using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16CertificateSignedResponse
/// </summary>
public class V16CertificateSignedResponseValidator : AbstractValidator<V16CertificateSignedResponse>
{
    public V16CertificateSignedResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
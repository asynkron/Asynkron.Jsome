using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16CertificateSigned
/// </summary>
public class V16CertificateSignedValidator : AbstractValidator<V16CertificateSigned>
{
    public V16CertificateSignedValidator()
    {
        RuleFor(x => x.CertificateChain)
            .NotEmpty.WithMessage("This field is required")
            .MaximumLength(10000).WithMessage("Must be no more than 10000 characters long")
            ;
    }
}
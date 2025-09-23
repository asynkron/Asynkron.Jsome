using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16SignCertificate
/// </summary>
public class V16SignCertificateValidator : AbstractValidator<V16SignCertificate>
{
    public V16SignCertificateValidator()
    {
        RuleFor(x => x.Csr)
            .NotEmpty.WithMessage("This field is required")
            .MaximumLength(5500).WithMessage("Must be no more than 5500 characters long")
            ;
    }
}
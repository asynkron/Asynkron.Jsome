using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16SignCertificateResponse
/// </summary>
public class V16SignCertificateResponseValidator : AbstractValidator<V16SignCertificateResponse>
{
    public V16SignCertificateResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
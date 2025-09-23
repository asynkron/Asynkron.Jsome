using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16DeleteCertificateResponse
/// </summary>
public class V16DeleteCertificateResponseValidator : AbstractValidator<V16DeleteCertificateResponse>
{
    public V16DeleteCertificateResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
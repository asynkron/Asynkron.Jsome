using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16GetDiagnosticsRequest
/// </summary>
public class V16GetDiagnosticsRequestValidator : AbstractValidator<V16GetDiagnosticsRequest>
{
    public V16GetDiagnosticsRequestValidator()
    {
        RuleFor(x => x.Location)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
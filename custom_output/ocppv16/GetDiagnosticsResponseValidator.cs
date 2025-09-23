using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16GetDiagnosticsResponse
/// </summary>
public class V16GetDiagnosticsResponseValidator : AbstractValidator<V16GetDiagnosticsResponse>
{
    public V16GetDiagnosticsResponseValidator()
    {
        RuleFor(x => x.FileName)
            .MaximumLength(255).WithMessage("Must be no more than 255 characters long")
            ;
    }
}
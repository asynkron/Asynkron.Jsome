using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16GetLog
/// </summary>
public class V16GetLogValidator : AbstractValidator<V16GetLog>
{
    public V16GetLogValidator()
    {
        RuleFor(x => x.Log)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.LogType)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.RequestId)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
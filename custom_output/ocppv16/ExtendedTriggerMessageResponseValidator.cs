using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16ExtendedTriggerMessageResponse
/// </summary>
public class V16ExtendedTriggerMessageResponseValidator : AbstractValidator<V16ExtendedTriggerMessageResponse>
{
    public V16ExtendedTriggerMessageResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
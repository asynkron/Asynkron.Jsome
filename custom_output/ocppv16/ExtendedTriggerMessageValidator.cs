using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16ExtendedTriggerMessage
/// </summary>
public class V16ExtendedTriggerMessageValidator : AbstractValidator<V16ExtendedTriggerMessage>
{
    public V16ExtendedTriggerMessageValidator()
    {
        RuleFor(x => x.RequestedMessage)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
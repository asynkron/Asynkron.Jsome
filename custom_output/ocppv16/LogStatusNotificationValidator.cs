using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16LogStatusNotification
/// </summary>
public class V16LogStatusNotificationValidator : AbstractValidator<V16LogStatusNotification>
{
    public V16LogStatusNotificationValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
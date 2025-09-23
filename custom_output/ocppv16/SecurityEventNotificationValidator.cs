using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16SecurityEventNotification
/// </summary>
public class V16SecurityEventNotificationValidator : AbstractValidator<V16SecurityEventNotification>
{
    public V16SecurityEventNotificationValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty.WithMessage("This field is required")
            .MaximumLength(50).WithMessage("Must be no more than 50 characters long")
            ;
        RuleFor(x => x.Timestamp)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.TechInfo)
            .MaximumLength(255).WithMessage("Must be no more than 255 characters long")
            ;
    }
}
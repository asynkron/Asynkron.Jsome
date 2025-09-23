using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16BootNotificationResponse
/// </summary>
public class V16BootNotificationResponseValidator : AbstractValidator<V16BootNotificationResponse>
{
    public V16BootNotificationResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "Accepted", "Pending", "Rejected" }.Contains(x.ToString())).WithMessage("Must be one of: "Accepted", "Pending", "Rejected"")
            ;
        RuleFor(x => x.CurrentTime)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.Interval)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
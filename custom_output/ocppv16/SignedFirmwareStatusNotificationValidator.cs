using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16SignedFirmwareStatusNotification
/// </summary>
public class V16SignedFirmwareStatusNotificationValidator : AbstractValidator<V16SignedFirmwareStatusNotification>
{
    public V16SignedFirmwareStatusNotificationValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
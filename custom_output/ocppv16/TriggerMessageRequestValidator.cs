using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16TriggerMessageRequest
/// </summary>
public class V16TriggerMessageRequestValidator : AbstractValidator<V16TriggerMessageRequest>
{
    public V16TriggerMessageRequestValidator()
    {
        RuleFor(x => x.RequestedMessage)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "BootNotification", "DiagnosticsStatusNotification", "FirmwareStatusNotification", "Heartbeat", "MeterValues", "StatusNotification" }.Contains(x.ToString())).WithMessage("Must be one of: "BootNotification", "DiagnosticsStatusNotification", "FirmwareStatusNotification", "Heartbeat", "MeterValues", "StatusNotification"")
            ;
    }
}
using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16StatusNotificationRequest
/// </summary>
public class V16StatusNotificationRequestValidator : AbstractValidator<V16StatusNotificationRequest>
{
    public V16StatusNotificationRequestValidator()
    {
        RuleFor(x => x.ConnectorId)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.ErrorCode)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "ConnectorLockFailure", "EVCommunicationError", "GroundFailure", "HighTemperature", "InternalError", "LocalListConflict", "NoError", "OtherError", "OverCurrentFailure", "PowerMeterFailure", "PowerSwitchFailure", "ReaderFailure", "ResetFailure", "UnderVoltage", "OverVoltage", "WeakSignal" }.Contains(x.ToString())).WithMessage("Must be one of: "ConnectorLockFailure", "EVCommunicationError", "GroundFailure", "HighTemperature", "InternalError", "LocalListConflict", "NoError", "OtherError", "OverCurrentFailure", "PowerMeterFailure", "PowerSwitchFailure", "ReaderFailure", "ResetFailure", "UnderVoltage", "OverVoltage", "WeakSignal"")
            ;
        RuleFor(x => x.Info)
            .MaximumLength(50).WithMessage("Must be no more than 50 characters long")
            ;
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "Available", "Preparing", "Charging", "SuspendedEVSE", "SuspendedEV", "Finishing", "Reserved", "Unavailable", "Faulted" }.Contains(x.ToString())).WithMessage("Must be one of: "Available", "Preparing", "Charging", "SuspendedEVSE", "SuspendedEV", "Finishing", "Reserved", "Unavailable", "Faulted"")
            ;
        RuleFor(x => x.VendorId)
            .MaximumLength(255).WithMessage("Must be no more than 255 characters long")
            ;
        RuleFor(x => x.VendorErrorCode)
            .MaximumLength(50).WithMessage("Must be no more than 50 characters long")
            ;
    }
}
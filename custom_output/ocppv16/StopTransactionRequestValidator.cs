using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16StopTransactionRequest
/// </summary>
public class V16StopTransactionRequestValidator : AbstractValidator<V16StopTransactionRequest>
{
    public V16StopTransactionRequestValidator()
    {
        RuleFor(x => x.IdTag)
            .MaximumLength(20).WithMessage("Must be no more than 20 characters long")
            ;
        RuleFor(x => x.MeterStop)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.Timestamp)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.TransactionId)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.Reason)
            .Must(x => new[] { "EmergencyStop", "EVDisconnected", "HardReset", "Local", "Other", "PowerLoss", "Reboot", "Remote", "SoftReset", "UnlockCommand", "DeAuthorized" }.Contains(x.ToString())).WithMessage("Must be one of: "EmergencyStop", "EVDisconnected", "HardReset", "Local", "Other", "PowerLoss", "Reboot", "Remote", "SoftReset", "UnlockCommand", "DeAuthorized"")
            ;
    }
}
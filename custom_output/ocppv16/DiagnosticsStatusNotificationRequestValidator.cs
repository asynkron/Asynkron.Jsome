using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16DiagnosticsStatusNotificationRequest
/// </summary>
public class V16DiagnosticsStatusNotificationRequestValidator : AbstractValidator<V16DiagnosticsStatusNotificationRequest>
{
    public V16DiagnosticsStatusNotificationRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "Idle", "Uploaded", "UploadFailed", "Uploading" }.Contains(x.ToString())).WithMessage("Must be one of: "Idle", "Uploaded", "UploadFailed", "Uploading"")
            ;
    }
}
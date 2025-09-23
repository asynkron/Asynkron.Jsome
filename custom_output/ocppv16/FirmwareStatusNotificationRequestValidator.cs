using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16FirmwareStatusNotificationRequest
/// </summary>
public class V16FirmwareStatusNotificationRequestValidator : AbstractValidator<V16FirmwareStatusNotificationRequest>
{
    public V16FirmwareStatusNotificationRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "Downloaded", "DownloadFailed", "Downloading", "Idle", "InstallationFailed", "Installing", "Installed" }.Contains(x.ToString())).WithMessage("Must be one of: "Downloaded", "DownloadFailed", "Downloading", "Idle", "InstallationFailed", "Installing", "Installed"")
            ;
    }
}
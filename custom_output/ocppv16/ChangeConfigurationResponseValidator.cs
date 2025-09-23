using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16ChangeConfigurationResponse
/// </summary>
public class V16ChangeConfigurationResponseValidator : AbstractValidator<V16ChangeConfigurationResponse>
{
    public V16ChangeConfigurationResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "Accepted", "Rejected", "RebootRequired", "NotSupported" }.Contains(x.ToString())).WithMessage("Must be one of: "Accepted", "Rejected", "RebootRequired", "NotSupported"")
            ;
    }
}
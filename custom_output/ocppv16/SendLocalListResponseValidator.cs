using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16SendLocalListResponse
/// </summary>
public class V16SendLocalListResponseValidator : AbstractValidator<V16SendLocalListResponse>
{
    public V16SendLocalListResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "Accepted", "Failed", "NotSupported", "VersionMismatch" }.Contains(x.ToString())).WithMessage("Must be one of: "Accepted", "Failed", "NotSupported", "VersionMismatch"")
            ;
    }
}
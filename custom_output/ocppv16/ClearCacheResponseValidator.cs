using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16ClearCacheResponse
/// </summary>
public class V16ClearCacheResponseValidator : AbstractValidator<V16ClearCacheResponse>
{
    public V16ClearCacheResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "Accepted", "Rejected" }.Contains(x.ToString())).WithMessage("Must be one of: "Accepted", "Rejected"")
            ;
    }
}
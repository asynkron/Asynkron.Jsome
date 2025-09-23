using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16SendLocalListRequest
/// </summary>
public class V16SendLocalListRequestValidator : AbstractValidator<V16SendLocalListRequest>
{
    public V16SendLocalListRequestValidator()
    {
        RuleFor(x => x.ListVersion)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.UpdateType)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "Differential", "Full" }.Contains(x.ToString())).WithMessage("Must be one of: "Differential", "Full"")
            ;
    }
}
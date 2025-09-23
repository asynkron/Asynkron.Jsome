using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16GetLocalListVersionResponse
/// </summary>
public class V16GetLocalListVersionResponseValidator : AbstractValidator<V16GetLocalListVersionResponse>
{
    public V16GetLocalListVersionResponseValidator()
    {
        RuleFor(x => x.ListVersion)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
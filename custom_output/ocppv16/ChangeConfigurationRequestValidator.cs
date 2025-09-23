using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16ChangeConfigurationRequest
/// </summary>
public class V16ChangeConfigurationRequestValidator : AbstractValidator<V16ChangeConfigurationRequest>
{
    public V16ChangeConfigurationRequestValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty.WithMessage("This field is required")
            .MinimumLength(1).WithMessage("Must be at least 1 characters long")
            .MaximumLength(50).WithMessage("Must be no more than 50 characters long")
            ;
        RuleFor(x => x.Value)
            .NotEmpty.WithMessage("This field is required")
            .MinimumLength(1).WithMessage("Must be at least 1 characters long")
            .MaximumLength(500).WithMessage("Must be no more than 500 characters long")
            ;
    }
}
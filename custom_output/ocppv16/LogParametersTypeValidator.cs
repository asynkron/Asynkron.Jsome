using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16LogParametersType
/// </summary>
public class V16LogParametersTypeValidator : AbstractValidator<V16LogParametersType>
{
    public V16LogParametersTypeValidator()
    {
        RuleFor(x => x.RemoteLocation)
            .NotEmpty.WithMessage("This field is required")
            .MaximumLength(512).WithMessage("Must be no more than 512 characters long")
            ;
    }
}
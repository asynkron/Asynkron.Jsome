using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16AuthorizeRequest
/// </summary>
public class V16AuthorizeRequestValidator : AbstractValidator<V16AuthorizeRequest>
{
    public V16AuthorizeRequestValidator()
    {
        RuleFor(x => x.IdTag)
            .NotEmpty.WithMessage("This field is required")
            .MaximumLength(20).WithMessage("Must be no more than 20 characters long")
            ;
    }
}
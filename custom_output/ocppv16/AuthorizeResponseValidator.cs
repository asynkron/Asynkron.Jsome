using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16AuthorizeResponse
/// </summary>
public class V16AuthorizeResponseValidator : AbstractValidator<V16AuthorizeResponse>
{
    public V16AuthorizeResponseValidator()
    {
        RuleFor(x => x.IdTagInfo)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
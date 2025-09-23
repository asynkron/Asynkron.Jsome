using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16StartTransactionRequest
/// </summary>
public class V16StartTransactionRequestValidator : AbstractValidator<V16StartTransactionRequest>
{
    public V16StartTransactionRequestValidator()
    {
        RuleFor(x => x.ConnectorId)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.IdTag)
            .NotEmpty.WithMessage("This field is required")
            .MaximumLength(20).WithMessage("Must be no more than 20 characters long")
            ;
        RuleFor(x => x.MeterStart)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.Timestamp)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
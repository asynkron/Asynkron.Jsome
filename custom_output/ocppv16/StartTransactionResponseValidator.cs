using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16StartTransactionResponse
/// </summary>
public class V16StartTransactionResponseValidator : AbstractValidator<V16StartTransactionResponse>
{
    public V16StartTransactionResponseValidator()
    {
        RuleFor(x => x.IdTagInfo)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.TransactionId)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
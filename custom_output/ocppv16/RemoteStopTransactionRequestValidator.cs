using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16RemoteStopTransactionRequest
/// </summary>
public class V16RemoteStopTransactionRequestValidator : AbstractValidator<V16RemoteStopTransactionRequest>
{
    public V16RemoteStopTransactionRequestValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
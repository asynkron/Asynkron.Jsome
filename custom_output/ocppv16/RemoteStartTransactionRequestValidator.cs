using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16RemoteStartTransactionRequest
/// </summary>
public class V16RemoteStartTransactionRequestValidator : AbstractValidator<V16RemoteStartTransactionRequest>
{
    public V16RemoteStartTransactionRequestValidator()
    {
        RuleFor(x => x.IdTag)
            .NotEmpty.WithMessage("This field is required")
            .MaximumLength(20).WithMessage("Must be no more than 20 characters long")
            ;
    }
}
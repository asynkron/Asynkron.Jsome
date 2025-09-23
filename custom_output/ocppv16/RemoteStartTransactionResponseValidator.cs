using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16RemoteStartTransactionResponse
/// </summary>
public class V16RemoteStartTransactionResponseValidator : AbstractValidator<V16RemoteStartTransactionResponse>
{
    public V16RemoteStartTransactionResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "Accepted", "Rejected" }.Contains(x.ToString())).WithMessage("Must be one of: "Accepted", "Rejected"")
            ;
    }
}
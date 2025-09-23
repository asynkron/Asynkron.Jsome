using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16RemoteStopTransactionResponse
/// </summary>
public class V16RemoteStopTransactionResponseValidator : AbstractValidator<V16RemoteStopTransactionResponse>
{
    public V16RemoteStopTransactionResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "Accepted", "Rejected" }.Contains(x.ToString())).WithMessage("Must be one of: "Accepted", "Rejected"")
            ;
    }
}
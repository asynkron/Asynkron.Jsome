using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16UnlockConnectorRequest
/// </summary>
public class V16UnlockConnectorRequestValidator : AbstractValidator<V16UnlockConnectorRequest>
{
    public V16UnlockConnectorRequestValidator()
    {
        RuleFor(x => x.ConnectorId)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16MeterValuesRequest
/// </summary>
public class V16MeterValuesRequestValidator : AbstractValidator<V16MeterValuesRequest>
{
    public V16MeterValuesRequestValidator()
    {
        RuleFor(x => x.ConnectorId)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.MeterValue)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => x.Count >= 1).WithMessage("Must contain at least 1 items")
            ;
    }
}
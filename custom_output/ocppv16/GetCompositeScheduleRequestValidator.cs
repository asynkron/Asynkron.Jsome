using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16GetCompositeScheduleRequest
/// </summary>
public class V16GetCompositeScheduleRequestValidator : AbstractValidator<V16GetCompositeScheduleRequest>
{
    public V16GetCompositeScheduleRequestValidator()
    {
        RuleFor(x => x.ConnectorId)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.Duration)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.ChargingRateUnit)
            .Must(x => new[] { "A", "W" }.Contains(x.ToString())).WithMessage("Must be one of: "A", "W"")
            ;
    }
}
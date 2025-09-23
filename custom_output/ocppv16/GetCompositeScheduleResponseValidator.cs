using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16GetCompositeScheduleResponse
/// </summary>
public class V16GetCompositeScheduleResponseValidator : AbstractValidator<V16GetCompositeScheduleResponse>
{
    public V16GetCompositeScheduleResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "Accepted", "Rejected" }.Contains(x.ToString())).WithMessage("Must be one of: "Accepted", "Rejected"")
            ;
    }
}
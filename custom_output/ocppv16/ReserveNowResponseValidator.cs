using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16ReserveNowResponse
/// </summary>
public class V16ReserveNowResponseValidator : AbstractValidator<V16ReserveNowResponse>
{
    public V16ReserveNowResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "Accepted", "Faulted", "Occupied", "Rejected", "Unavailable" }.Contains(x.ToString())).WithMessage("Must be one of: "Accepted", "Faulted", "Occupied", "Rejected", "Unavailable"")
            ;
    }
}
using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16ChangeAvailabilityResponse
/// </summary>
public class V16ChangeAvailabilityResponseValidator : AbstractValidator<V16ChangeAvailabilityResponse>
{
    public V16ChangeAvailabilityResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "Accepted", "Rejected", "Scheduled" }.Contains(x.ToString())).WithMessage("Must be one of: "Accepted", "Rejected", "Scheduled"")
            ;
    }
}
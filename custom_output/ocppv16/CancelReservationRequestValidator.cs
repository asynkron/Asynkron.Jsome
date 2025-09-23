using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16CancelReservationRequest
/// </summary>
public class V16CancelReservationRequestValidator : AbstractValidator<V16CancelReservationRequest>
{
    public V16CancelReservationRequestValidator()
    {
        RuleFor(x => x.ReservationId)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
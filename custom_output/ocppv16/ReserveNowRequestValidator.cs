using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16ReserveNowRequest
/// </summary>
public class V16ReserveNowRequestValidator : AbstractValidator<V16ReserveNowRequest>
{
    public V16ReserveNowRequestValidator()
    {
        RuleFor(x => x.ConnectorId)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.ExpiryDate)
            .NotEmpty.WithMessage("This field is required")
            ;
        RuleFor(x => x.IdTag)
            .NotEmpty.WithMessage("This field is required")
            .MaximumLength(20).WithMessage("Must be no more than 20 characters long")
            ;
        RuleFor(x => x.ParentIdTag)
            .MaximumLength(20).WithMessage("Must be no more than 20 characters long")
            ;
        RuleFor(x => x.ReservationId)
            .NotEmpty.WithMessage("This field is required")
            ;
    }
}
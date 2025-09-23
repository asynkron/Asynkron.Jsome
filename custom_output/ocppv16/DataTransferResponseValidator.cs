using FluentValidation;

namespace OCPP.V16.Generated.Validators;

/// <summary>
/// Validator for V16DataTransferResponse
/// </summary>
public class V16DataTransferResponseValidator : AbstractValidator<V16DataTransferResponse>
{
    public V16DataTransferResponseValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty.WithMessage("This field is required")
            .Must(x => new[] { "Accepted", "Rejected", "UnknownMessageId", "UnknownVendorId" }.Contains(x.ToString())).WithMessage("Must be one of: "Accepted", "Rejected", "UnknownMessageId", "UnknownVendorId"")
            ;
    }
}
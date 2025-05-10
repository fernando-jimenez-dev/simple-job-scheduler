using Application.Shared.Errors;

namespace Application.UseCases.CheckPulse.Errors;

public record EmptyVitalsError() : Error("No vitals were found.")
{
}
using FluentResults;

namespace Application.UseCases.CheckPulse.Errors;

public class EmptyVitalsError() : Error("No vitals were found.")
{
}
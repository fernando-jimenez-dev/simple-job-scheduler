using Application.UseCases.ExecutePowerShell.Abstractions;
using static Application.UseCases.ExecutePowerShell.Abstractions.IExecutePowerShellUseCase;

namespace Application.Shared;

/// <summary>
/// Creates the relationship between a Guid and a UseCase that can be triggered as a Job Type.
/// Not all use cases will be here - just those that need to be executed as jobs.
///
/// Use cases will be assigned a unique and eternal Guid. Hence, this configuration like class
/// should never be touched to change any Guids once they are set.
/// </summary>
public sealed class JobTypeRegistry
{
    public static readonly UseCaseType ExecutePowerShellType = new(
        Guid.Parse("ef9c5dbf-c3e9-48c9-bc13-f35c21f4cabd"), typeof(IExecutePowerShellUseCase), typeof(ExecutePowerShellInput)
    );

    private static readonly Dictionary<Guid, UseCaseType> _registry = new()
    {
        { ExecutePowerShellType.Id, ExecutePowerShellType },
    };

    public static readonly IReadOnlyDictionary<Guid, UseCaseType> Registry = _registry;

    public static bool JobExists(Guid id) => Registry.ContainsKey(id);

    public record UseCaseType(Guid Id, Type UseCaseInterface, Type UseCaseInput);
}
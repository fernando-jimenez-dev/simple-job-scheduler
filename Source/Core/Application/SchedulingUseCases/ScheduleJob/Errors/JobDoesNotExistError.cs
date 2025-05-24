using Application.Shared.Errors;
using Microsoft.Extensions.Logging;

namespace Application.SchedulingUseCases.ScheduleJob.Errors;

public record JobDoesNotExistError : ApplicationError
{
    public Guid JobId { get; init; }

    public override LogLevel Severity => LogLevel.Warning;
    public const string Name = nameof(JobDoesNotExistError);

    public JobDoesNotExistError(Guid jobId, Exception? exception = null)
        : base(Name, $"Job with Id {jobId} does not exist.", exception)
    {
        JobId = jobId;
    }

    public static JobDoesNotExistError For(Guid jobId, Exception? exception = null) => new(jobId, exception);
}
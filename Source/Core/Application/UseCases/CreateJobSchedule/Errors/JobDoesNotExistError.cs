using Application.Shared.Errors;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.CreateJobSchedule.Errors;

public class JobDoesNotExistError : ApplicationError
{
    public Guid JobId { get; init; }

    public override LogLevel Severity => LogLevel.Warning;

    public JobDoesNotExistError(Guid jobId, Exception? exception = null)
        : base(nameof(JobDoesNotExistError), $"Job with Id {jobId} does not exist.", exception)
    {
        JobId = jobId;
    }

    public static JobDoesNotExistError For(Guid jobId, Exception? exception = null) => new(jobId, exception);
}
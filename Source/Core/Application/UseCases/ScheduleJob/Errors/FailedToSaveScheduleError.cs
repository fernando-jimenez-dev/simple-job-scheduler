using Application.Shared.Errors;
using Microsoft.Extensions.Logging;
using OpenResult;

namespace Application.UseCases.ScheduleJob.Errors;

public record FailedToSaveScheduleError : ApplicationError
{
    public Result<int> SaveScheduleResult { get; init; }

    public override LogLevel Severity => LogLevel.Error;

    public const string Name = nameof(FailedToSaveScheduleError);

    public FailedToSaveScheduleError(Result<int> saveScheduleResult, Exception? exception = null)
        : base(
            Name,
            "There was an error while scheduling the job.",
             exception
        )
    {
        SaveScheduleResult = saveScheduleResult;
    }

    public static FailedToSaveScheduleError For(Result<int> saveScheduleResult, Exception? exception = null)
        => new(saveScheduleResult, exception);
}
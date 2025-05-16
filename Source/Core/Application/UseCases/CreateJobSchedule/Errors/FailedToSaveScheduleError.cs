using Application.Shared.Errors;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.CreateJobSchedule.Errors;

public class FailedToSaveScheduleError : ApplicationError
{
    public Result<int> SaveScheduleResult { get; init; }

    public override LogLevel Severity => LogLevel.Error;

    public FailedToSaveScheduleError(Result<int> saveScheduleResult, Exception? exception = null)
        : base(
            nameof(FailedToSaveScheduleError),
            "There was an error while scheduling the job.",
             exception
        )
    {
        SaveScheduleResult = saveScheduleResult;
    }

    public static FailedToSaveScheduleError For(Result<int> saveScheduleResult, Exception? exception = null)
        => new(saveScheduleResult, exception);
}
using Application.UseCases.CreateJobSchedule;
using static WebAPI.Minimal.UseCases.CreateJobSchedule.RequestSchedule;

namespace WebAPI.Minimal.UseCases.CreateJobSchedule;

public record CreateJobScheduleRequest(Guid JobId, RequestSchedule Schedule, string? Parameters = null)
{
    public CreateJobScheduleInput ToInputOrDefault()
    {
        var schedule = Schedule.ToDomainOrDefault();
        return new CreateJobScheduleInput(schedule, JobId, Parameters);
    }
}

public record RequestSchedule(ScheduleType Type, string Value)
{
    public enum ScheduleType
    {
        OneTime = 1,
        CronSchedule = 2
    }

    /// <summary>
    /// Maps the incoming
    /// </summary>
    public Schedule ToDomainOrDefault()
    {
        /// This default value SHOULD be rejected by the Use Case Input validation logic.
        /// If it isn't we have bigger issues to deal with.
        var defaultInvalidValue = new OneTimeSchedule(DateTimeOffset.MinValue);

        switch (Type)
        {
            case ScheduleType.OneTime:
                {
                    if (DateTimeOffset.TryParse(Value, out var datetimeOffset))
                        return new OneTimeSchedule(datetimeOffset);

                    break;
                }
            case ScheduleType.CronSchedule:
                {
                    return new CronSchedule(Value);
                }
        }
        return defaultInvalidValue;
    }
}
using FluentValidation;
using FluentValidation.Results;

namespace Application.UseCases.ScheduleJob;

/// <param name="Schedule">Time in the future for the job to be executed.</param>
/// <param name="JobId">Job type to be executed.</param>
/// <param name="Parameters">Parameters for the job to use during execution as JSON.</param>
public record ScheduleJobInput(Schedule Schedule, Guid JobId, string? Parameters = null)
{
    public ValidationResult Validate() => new Validator().Validate(this);
    public class Validator : AbstractValidator<ScheduleJobInput>
    {
        public Validator()
        {
            RuleFor(x => x.JobId).NotEmpty(); // Eventually it should match an existing Id?
            RuleFor(x => x.Schedule).Must(s => s is OneTimeSchedule || s is CronSchedule).WithMessage("Unknown schedule type.");
            RuleFor(x => x.Schedule).SetInheritanceValidator(v =>
            {
                //use the validator dedicated to the type of schedule.
                v.Add(new OneTimeSchedule.Validator());
                v.Add(new CronSchedule.Validator());
            });
            RuleFor(x => x.Parameters)
                .Must(BeValidJson)
                .When(x => x.Parameters != null)
                .WithMessage("Parameters must be a valid JSON string.");
        }

        private static bool BeValidJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return true;
            try
            {
                System.Text.Json.JsonDocument.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

public abstract record Schedule
{
    public abstract ValidationResult Validate();
}

public record OneTimeSchedule(DateTimeOffset ScheduledAt) : Schedule
{
    public override ValidationResult Validate() => new Validator().Validate(this);
    public class Validator : AbstractValidator<OneTimeSchedule>
    {
        public Validator()
        {
            RuleFor(x => x.ScheduledAt)
                .Must(BeInTheFuture)
                .WithMessage("ScheduledAt must be in the future.");
        }
    }

    private static bool BeInTheFuture(DateTimeOffset scheduledAt)
        => scheduledAt > DateTimeOffset.UtcNow.AddSeconds(1); // Add a 1s buffer.
}

public record CronSchedule(string CronExpression) : Schedule
{
    public override ValidationResult Validate() => new Validator().Validate(this);
    public class Validator : AbstractValidator<CronSchedule>
    {
        public Validator()
        {
            RuleFor(x => x.CronExpression)
                .NotEmpty()
                .Must(BeValidCron)
                .WithMessage("Cron expression is invalid.");
        }

        public static bool BeValidCron(string expr)
        {
            if (string.IsNullOrWhiteSpace(expr))
                return false;

            // Remove extra spaces, split on spaces
            var fields = expr.Trim().Split([' '], StringSplitOptions.RemoveEmptyEntries);

            try
            {
                // 6 fields = with seconds, 5 fields = standard cron
                if (fields.Length == 6)
                    Cronos.CronExpression.Parse(expr, Cronos.CronFormat.IncludeSeconds);
                else if (fields.Length == 5)
                    Cronos.CronExpression.Parse(expr, Cronos.CronFormat.Standard);
                else
                    return false; // Invalid field count

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
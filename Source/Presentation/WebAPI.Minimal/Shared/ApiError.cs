using Application.Shared.Errors;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebAPI.Minimal.Shared;

public record ApiError
{
    public Guid Id { get; init; }
    public string Type { get; init; }
    public string Message { get; init; }
    public Dictionary<string, object?> Details { get; init; }

    [JsonIgnore]
    public LogLevel Severity { get; init; }

    [JsonIgnore]
    public Exception? Exception { get; init; }

    private ApiError(ApplicationError error)
    {
        Id = error.Id;
        Type = error.Type;
        Message = error.Message;
        Details = [];
        Exception = error.Exception;
        Severity = error.Severity;
    }

    public static ApiError FromApplicationError<T>(ApplicationError error, T request)
    {
        var apiError = new ApiError(error).WithDetail("request", request);
        return apiError;
    }

    public ApiError WithDetail(string name, object? value)
    {
        Details[name] = value;
        return this;

        /*
         * Keeping the Dictionary Immutable - creating a new one instead version. I'll decide later.
         *
            var copy = this with { Details = new Dictionary<string, object?>(this.Details) };
            copy.Details[name] = value;
            return copy;
         */
    }

    public override string ToString()
    {
        try
        {
            return JsonSerializer.Serialize(this, ApiErrorJsonSerializerOptions.Options);
        }
        catch (Exception ex)
        {
            return $"{{ \"SerializationFailure\": \"{ex.Message}\" }}";
        }
    }
}

public class ApiErrorJsonSerializerOptions
{
    public static JsonSerializerOptions Options => new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };
}
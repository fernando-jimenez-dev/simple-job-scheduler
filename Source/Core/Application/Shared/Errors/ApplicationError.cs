using FluentResults;
using Microsoft.Extensions.Logging;

namespace Application.Shared.Errors;

//public abstract class ApplicationError(string Message, string ErrorId, string ErrorType) : Error(Message)
//{
//    public string Id => Guid.NewGuid().ToString();
//    public string Type => ErrorType;
//}

public class ApplicationError : Error, IError
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public Exception? Exception { get; set; }

    /// <summary>
    /// All Errors will be logged at some point during the lifecycle of the application.
    /// It is important to make them declare their severity level, as that would change the way
    /// our monitor system acts on them.
    ///
    /// ERROR means the error will be marked for Review.
    /// WARNING doesn't get picked up automatically.
    /// FATAL means the application is unresponsive if this is logged.
    ///
    /// If your Error type is more of a Informational Error (Warning), then override Severity to Warning.
    /// </summary>
    public virtual LogLevel Severity => LogLevel.Error;

    public ApplicationError(string type, string message, Exception? exception = null) : base(message)
    {
        Id = Guid.NewGuid();
        Type = type;
        Exception = exception;
    }
}
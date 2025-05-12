using FluentResults;

namespace Application.Shared.Errors;

public class OldUnexpectedError(string Message, Exception Exception) : Error(Message);

public class UnexpectedError : ApplicationError
{
    public Exception? Exception { get; }

    public UnexpectedError(string message, Exception? exception = null) : base(nameof(UnexpectedError), message)
    {
        Exception = exception;
    }
}
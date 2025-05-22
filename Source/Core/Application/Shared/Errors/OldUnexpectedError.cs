using OpenResult;

namespace Application.Shared.Errors;

public record OldUnexpectedError(string Message, Exception Exception) : Error(Message, Exception: Exception);

public record UnexpectedError : ApplicationError
{
    public UnexpectedError(string message, Exception? exception = null) : base(nameof(UnexpectedError), message, exception)
    {
    }
}
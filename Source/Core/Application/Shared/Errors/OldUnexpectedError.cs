using FluentResults;
using Microsoft.Extensions.Logging;

namespace Application.Shared.Errors;

public class OldUnexpectedError(string Message, Exception Exception) : Error(Message);

public class UnexpectedError : ApplicationError
{
    public UnexpectedError(string message, Exception? exception = null) : base(nameof(UnexpectedError), message, exception)
    {
    }
}
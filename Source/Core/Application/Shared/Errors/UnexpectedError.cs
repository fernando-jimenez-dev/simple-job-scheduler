namespace Application.Shared.Errors;

public record UnexpectedError(string Message, Exception Exception) : Error(Message);
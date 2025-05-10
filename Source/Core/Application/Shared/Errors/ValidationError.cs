namespace Application.Shared.Errors;

public record ValidationError(string Message, string ValidatedObject) : Error(Message);
using OpenResult;

namespace Application.Shared.Errors;

public record OldValidationError(string Message, string ValidatedObject) : Error(Message);
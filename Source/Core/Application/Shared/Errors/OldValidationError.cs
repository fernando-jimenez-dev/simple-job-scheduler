using FluentResults;

namespace Application.Shared.Errors;

public class OldValidationError(string Message, string ValidatedObject) : Error(Message);
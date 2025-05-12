using FluentResults;

namespace Application.Shared.Errors;

public class ValidationError(string Message, string ValidatedObject) : Error(Message);
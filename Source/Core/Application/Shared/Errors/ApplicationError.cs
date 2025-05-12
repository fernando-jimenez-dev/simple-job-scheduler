using FluentResults;

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

    public ApplicationError(string type, string message) : base(message)
    {
        Id = Guid.NewGuid();
        Type = type;
    }
}
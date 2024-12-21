namespace Neurointegration.Api.Excpetions;

public record Error
{
    public ErrorStatus Status { get; private set; }
    public int Code { get; private set; }
    public string Message { get; private set; }

    public static Error NotFound(string message) => new Error()
    {
        Status = ErrorStatus.NotFound,
        Code = 404,
        Message = message
    };
    
    public static Error InnerError(string message) => new Error()
    {
        Status = ErrorStatus.InnerError,
        Code = 500,
        Message = message
    };
}
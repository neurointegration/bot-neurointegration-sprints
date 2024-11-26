namespace Neurointegration.Api.Excpetions;

public class ResponseExceptionDto
{
    public ResponseExceptionDto(int statusCode, string message)
    {
        Message = message;
        StatusCode = statusCode;
    }

    public ResponseExceptionDto()
    {
    }

    public string Message { get; set; }
    public int StatusCode { get; set; }
}
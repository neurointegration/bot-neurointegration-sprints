namespace Neurointegration.Api.Excpetions;

public class DataConflictException: Exception
{
    public DataConflictException(string message) : base(message)
    {
    }
}
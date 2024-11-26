namespace BotTemplate.Services.Telegram.Commands;

public enum CommandStatus
{
    Success, Fail
}

public class CommandResponse<T>
{
    public CommandStatus CommandStatus { get; private set; }
    public T? Response { get; private set; }

    public static CommandResponse<T> CreateSuccessfulCommandResponse(T response)
    {
        return new CommandResponse<T> { CommandStatus = CommandStatus.Success, Response = response };
    }
    
    public static CommandResponse<T> CreateSuccessfulCommandResponse()
    {
        return new CommandResponse<T> { CommandStatus = CommandStatus.Success, Response = default };
    }
    
    public static CommandResponse<T> CreateFailedCommandResponse(T? response)
    {
        return new CommandResponse<T> { CommandStatus = CommandStatus.Fail, Response = response };
    }
    
    public static CommandResponse<T> CreateFailedCommandResponse()
    {
        return new CommandResponse<T> { CommandStatus = CommandStatus.Fail, Response = default };
    }

    public bool IsSuccessful()
    {
        return CommandStatus is CommandStatus.Success;
    }
}
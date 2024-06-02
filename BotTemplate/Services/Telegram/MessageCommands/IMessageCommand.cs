using BotTemplate.Services.Telegram.Commands;

namespace BotTemplate.Services.Telegram.MessageCommands;

public interface IMessageCommand
{
    string Command { get; }
    Task<CommandResponse<string>> Handle(IMessageView messageView, long chatId, string? userMessage);
}
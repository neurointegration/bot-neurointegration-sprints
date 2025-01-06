using BotTemplate.Services.Telegram.Commands;

namespace BotTemplate.Services.Telegram.Messages.Status;

public interface IMessageCommand
{
    string Command { get; }
    Task<CommandResponse<string>> Handle(IMessageSender messageSender, long chatId, string? userMessage);
}
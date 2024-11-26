using Telegram.Bot.Types.Enums;

namespace BotTemplate.Models.Telegram;

public class TelegramEvent
{
    public string? Text { get; set; }
    public string? Username { get; set; }
    public long ChatId { get; set; }
    public MessageType MessageType { get; set; }
}
using BotTemplate.Models.Telegram;

namespace BotTemplate.Services.Telegram.Messages.Settings;

public static class ChangeStatusTimeMessage
{
    private const string Text = "Укажи новый временной промежуток по МСК, когда тебе можно отправлять уведомления о состоянии";
    
    public static Message GetMessage()
    {
        return new Message(Text);
    }
}
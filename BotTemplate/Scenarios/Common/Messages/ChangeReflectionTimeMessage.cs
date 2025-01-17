using BotTemplate.Models.Telegram;

namespace BotTemplate.Scenarios.Common.Messages;

public static class ChangeReflectionTimeMessage
{
    private const string Text = "Укажи новое время по МСК, когда тебе можно отправлять напоминания о вечерней рефлексии";
    
    public static Message GetMessage()
    {
        return new Message(Text);
    }
}
using BotTemplate.Models.Telegram;

namespace BotTemplate.Scenarios.User.Settings;

public static class ChangeEveningStandUpTimeMessage
{
    private const string Text = "Укажи новое время по МСК, когда тебе можно отправлять напоминания о вечернем стендапе";
    
    public static Message GetMessage()
    {
        return new Message(Text);
    }
}
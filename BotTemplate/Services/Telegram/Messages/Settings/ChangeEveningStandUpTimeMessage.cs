using BotTemplate.Models.Telegram;

namespace BotTemplate.Services.Telegram.Messages.Settings;

public static class ChangeEveningStandUpTimeMessage
{
    private const string Text = "Укажи новое время, когда тебе можно отправлять напоминания о вечернем стендапе.";
    
    public static Message GetMessage()
    {
        return new Message(Text);
    }
}
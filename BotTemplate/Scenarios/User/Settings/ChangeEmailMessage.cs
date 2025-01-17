using BotTemplate.Models.Telegram;

namespace BotTemplate.Services.Telegram.Messages.Settings;

public static class ChangeEmailMessage
{
    private const string Text = "Укажи новую почту";
    
    public static Message GetMessage()
    {
        return new Message(Text);
    }
}
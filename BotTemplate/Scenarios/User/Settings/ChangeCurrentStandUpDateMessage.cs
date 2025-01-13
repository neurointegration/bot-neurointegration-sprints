using BotTemplate.Models.Telegram;

namespace BotTemplate.Services.Telegram.Messages.Settings;

public static class ChangeCurrentStandUpDateMessage
{
    private const string Text = "Укажи дату, когда начался текущий спринт.";
    
    public static Message GetMessage()
    {
        return new Message(Text);
    }
}
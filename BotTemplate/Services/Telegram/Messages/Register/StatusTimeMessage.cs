using BotTemplate.Models.Telegram;

namespace BotTemplate.Services.Telegram.Messages.Register;

public static class StatusTimeMessage
{
    private const string Text = "Напиши, пожалуйста, временной промежуток, когда тебе можно отправлять уведомления.\n " +
                                "Укажи интервал времени по МСК через - , например, 9:00-18:00";
    
    public static Message GetMessage()
    {
        return new Message(Text, null);
    }
}
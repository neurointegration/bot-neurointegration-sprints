using BotTemplate.Models.Telegram;

namespace BotTemplate.Services.Telegram.Messages.Register;

public static class EveningStandUpTimeMessage
{
    private const string Text = "Отлично! Буду писать только в это время. " +
                                "А во сколько поставить уведомление о вечернем стендапе? Укажи только время по МСК. К примеру, 19:00";
    
    public static Message GetMessage()
    {
        return new Message(Text, null);
    }
}
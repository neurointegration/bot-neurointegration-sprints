using BotTemplate.Models.Telegram;

namespace BotTemplate.Services.Telegram.Messages.Register;

public static class WriteYourEmailMessage
{
    private const string Text = "Привет! Меня зовут Баланси. Я буду помогать тебе в прохождении " +
                                "<a href='https://ru.neurointegration.org/science'>спринтов нейроинтеграции</a>. " +
                                "Прежде чем начать нашу работу, я задам тебе пару вопросов. \n " +
                                "Для начала укажи свою <b>почту</b>, у которой существует Google аккаунт. " +
                                "Это нужно для того, чтобы я мог выдать тебе доступ к таблицам твоих спринтов";
    
    public static Message GetMessage()
    {
        return new Message(Text);
    }
}
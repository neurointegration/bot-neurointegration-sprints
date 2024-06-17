using BotTemplate.Models.Telegram;

namespace BotTemplate.Services.Telegram.Messages.Register;

public static class RegisteredMessage
{
    private const string Text = "Спасибо за твои ответы! " +
                                "Я буду отправлять в неожиданное для тебя время вопрос “Как ты себя сейчас чувствуешь?” с вариантами ответа. " +
                                "Также буду напоминать заполнить вечерний стендап и рефлексию. " +
                                "Твои ответы я буду записывать в Excel-табличку. Рад знакомству!";
    
    public static Message GetMessage()
    {
        return new Message(Text);
    }
}
using BotTemplate.Models.Telegram;

namespace BotTemplate.Services.Telegram.Messages.Register;

public static class RegisterNoSprintsMessage
{
    private const string Text = "Спасибо за твои ответы! Если ты все же решишь проходить спринты, то ты можешь изменить это в настройках. Рад знакомству!";

    public static Message GetMessage()
    {
        return new Message(Text);
    }
}
using BotTemplate.Models;
using BotTemplate.Models.Telegram;
using Newtonsoft.Json;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotTemplate.Services.Telegram.Messages.Register;

public static class AreYouACoachMessage
{
    private const string Text = "Ты - тренер?";

    public static Message GetMessage(string email)
    {
        var text = $"Отлично! Я выдам доступ к таблицам Google аккаунту с почтой {email}. {Text}";

        // Buttons
        var yesButton = new InlineKeyboardButton("Да");
        var noButton = new InlineKeyboardButton("Нет");

        yesButton.CallbackData = "Да";
        noButton.CallbackData = "Нет";

        var buttons = new[] {yesButton, noButton};

        // Keyboard markup
        var inline = new InlineKeyboardMarkup(buttons);

        return new Message(text, inline);
    }
}
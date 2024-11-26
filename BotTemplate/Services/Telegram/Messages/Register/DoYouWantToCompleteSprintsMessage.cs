using BotTemplate.Models.Telegram;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotTemplate.Services.Telegram.Messages.Register;

public static class DoYouWantToCompleteSprintsMessage
{
    private const string Text = "Хочешь проходить спринты?";
    
    public static Message GetMessage()
    {
        // Buttons
        var yesButton = new InlineKeyboardButton("Да");
        var noButton = new InlineKeyboardButton("Нет");

        yesButton.CallbackData = "Да";
        noButton.CallbackData = "Нет";

        var buttons = new[] { yesButton, noButton };
    
        // Keyboard markup
        var inline = new InlineKeyboardMarkup(buttons);

        return new Message(Text, inline);
    }
}
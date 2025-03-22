using BotTemplate.Models.Telegram;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotTemplate.Services.Telegram.Messages.Register;

public static class CurrentStandUpDateStart
{
    private const string Text = "Хорошо, а что насчет текущего спринта? " +
                                "Если в данный момент ты уже проходишь спринт, то напиши дату его начала в формате 06.01.2025";
    
    public static Message GetMessage()
    {
        var newSprintButton = new InlineKeyboardButton($"Новый спринт {DateTime.UtcNow:dd.MM.yyyy}");

        newSprintButton.CallbackData = "Новый спринт";

        var buttons = new[] { newSprintButton };
        
        var inline = new InlineKeyboardMarkup(buttons);

        return new Message(Text, inline);
    }
}
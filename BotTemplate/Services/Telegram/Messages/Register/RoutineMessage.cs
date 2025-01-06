using BotTemplate.Models.Telegram;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotTemplate.Services.Telegram.Messages.Register;

public static class RoutineMessage
{
    public static Message GetMessage()
    {
        var text = $"У тебя уже есть привычки. Можешь написать их в формате 'Тип Название'. Например: Лайв Лечь спать в 22:00.";
        
        // Buttons
        var cancelButton = new InlineKeyboardButton("Завершить");

        cancelButton.CallbackData = MessageConstants.EndCompleteRoutineActions;
    
        var buttons = new[] { cancelButton };
        // Keyboard markup
        var inline = new InlineKeyboardMarkup(buttons);

        return new Message(text, inline);
    }
    
    public static Message GetContinueMessage()
    {
        var text = $"Записал! Можешь добавить еще или завершить.";
        
        // Buttons
        var cancelButton = new InlineKeyboardButton("Завершить");

        cancelButton.CallbackData = MessageConstants.EndCompleteRoutineActions;
    
        var buttons = new[] { cancelButton };
        // Keyboard markup
        var inline = new InlineKeyboardMarkup(buttons);

        return new Message(text, inline);
    }
}
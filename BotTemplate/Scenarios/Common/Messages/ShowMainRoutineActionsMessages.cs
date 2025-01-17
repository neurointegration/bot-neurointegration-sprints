using BotTemplate.Models.Telegram;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotTemplate.Scenarios.Common.Messages;

public static class ShowMainRoutineActionsMessages
{
    public static Message GetMessage()
    {
        var checkupRoutine = new[]
        {
            new InlineKeyboardButton("Отметить")
            {
                CallbackData = CommandsConstants.StartCheckupRoutineActions
            }
        };
        var changeRoutine = new[]
        {
            new InlineKeyboardButton("Изменить")
            {
                CallbackData = CommandsConstants.ChangeRoutineActions
            }
        };

        var buttons = new List<InlineKeyboardButton[]>
        {
            changeRoutine,
            checkupRoutine,
        };

        var inline = new InlineKeyboardMarkup(buttons);
        var text = "Что делаем с рутинными делами?";

        return new Message(text, inline);
    }
}
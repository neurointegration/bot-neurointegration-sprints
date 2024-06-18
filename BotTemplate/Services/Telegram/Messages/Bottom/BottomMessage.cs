using BotTemplate.Models.Telegram;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotTemplate.Services.Telegram.Messages.Bottom;

public static class BottomMessage
{
    private const string Text = "Меню";

    public static Message GetMessage(bool iAmCoach = false)
    {
        var tableButton = new KeyboardButton("Таблица результатов");
        var settingsButton = new KeyboardButton("Настройки");
        var myStudents = new KeyboardButton("Мои ученики");

        KeyboardButton[][] buttons;
        if (iAmCoach)
            buttons = new[]
            {
                new[] { tableButton },
                new[] { settingsButton },
                new[] { myStudents }
            };
        else
        {
            buttons = new[]
            {
                new[] { tableButton },
                new[] { settingsButton }
            };
        }
        var keyboard = new ReplyKeyboardMarkup(buttons)
        {
            ResizeKeyboard = true
        };

        return new Message(Text, keyboard);
    }
}
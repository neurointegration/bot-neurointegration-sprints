using BotTemplate.Models.Telegram;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotTemplate.Scenarios.User;

public static class MyInfoMessages
{
    public static Message GetMessage(bool iAmCoach = false)
    {
        var tableButton = new InlineKeyboardButton("Таблица результатов") {CallbackData = CommandsConstants.ResultTablesCommand};
        var myStudents = new InlineKeyboardButton("Мои ученики") {CallbackData = CommandsConstants.GetStudents};
        var routineActions = new InlineKeyboardButton("Отметить рутинные действия") {CallbackData = CommandsConstants.StartCheckupRoutineActions};
        var changeRoutineActions = new InlineKeyboardButton("Изменить рутинные действия") {CallbackData = CommandsConstants.ChangeRoutineActions};

        var buttons = new List<InlineKeyboardButton[]>
        {
            new []{tableButton},
            new []{routineActions},
            new []{changeRoutineActions}
        };
        
        if (iAmCoach)
            buttons.Add(new []{myStudents});

        var inline = new InlineKeyboardMarkup(buttons);

        return new Message("Меню", inline);
    }
}
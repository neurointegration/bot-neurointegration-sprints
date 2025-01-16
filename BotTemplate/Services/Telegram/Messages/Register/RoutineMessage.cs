using BotTemplate.Models.Telegram;
using Neurointegration.Api.DataModels.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotTemplate.Services.Telegram.Messages.Register;

public static class RoutineMessage
{
    public static Message CurrentRoutines(List<WeekRoutineAction> routineActions)
    {
        var listActions = routineActions.Select((action, i) => action with
        {
            Action = $"- {action.Action}. Удалить: {CommandsConstants.DeleteRoutineAction(i)}"
        }).ToArray();
        var lifeActions = listActions.Where(action => action.Type == RoutineType.Life).Select(x => x.Action).ToArray();
        var driveActions = listActions.Where(action => action.Type == RoutineType.Drive).Select(x => x.Action)
            .ToArray();
        var pleasureActions = listActions.Where(action => action.Type == RoutineType.Pleasure).Select(x => x.Action)
            .ToArray();


        var text = $@"Твои привычки:
🚶‍♂️ Лайф. Добавить новые: {CommandsConstants.AddRoutineAction(RoutineType.Life)}
{string.Join("\n", lifeActions)}{(lifeActions.Length != 0 ? "\n" : "")}
🚀 Драйв. Добавить новые: {CommandsConstants.AddRoutineAction(RoutineType.Drive)}
{string.Join("\n", driveActions)}{(driveActions.Length != 0 ? "\n" : "")}
💆‍♀️ Кайф. Добавить новые: {CommandsConstants.AddRoutineAction(RoutineType.Pleasure)}
{string.Join("\n", pleasureActions)}{(pleasureActions.Length != 0 ? "\n" : "")}";

        var cancelButton = new InlineKeyboardButton("Завершить")
        {
            CallbackData = CommandsConstants.CancelEditRoutineActions
        };

        var buttons = new[] {cancelButton};
        var inline = new InlineKeyboardMarkup(buttons);

        return new Message(text, inline);
    }

    public static Message AddRoutinesMessage(RoutineType routineType)
    {
        var type = routineType switch
        {
            RoutineType.Life => "лайфа",
            RoutineType.Pleasure => "кайфа",
            RoutineType.Drive => "драйва",
            _ => throw new ArgumentOutOfRangeException(nameof(routineType), routineType, null)
        };
        var text =
            $"Напиши новые рутинные действия для {type}. Одним сообщением, каждое действие на отдельной строчке";

        var cancelButton = new InlineKeyboardButton("Вернуться к списку")
        {
            CallbackData = CommandsConstants.ReturnToRoutineActionsList
        };

        var buttons = new[] {cancelButton};
        var inline = new InlineKeyboardMarkup(buttons);

        return new Message(text, inline);
    }

    public static Message FinishChangeRoutineActions()
    {
        var text = $"Рутинные действия успешно изменены!";
        var buttons = Array.Empty<InlineKeyboardButton>();
        var inline = new InlineKeyboardMarkup(buttons);

        return new Message(text, inline);
    }

    public static Message RoutineWeekendStatus(List<WeekRoutineAction> routineActions)
    {
        var text = "Отметь рутинные действия, которые сделал за сегодня!";
        var buttons = routineActions.Select((action) =>
                new[]
                {
                    new InlineKeyboardButton($"{action.Action} {action.WeekCount}/7")
                    {
                        CallbackData = CommandsConstants.CheckupRoutineAction(action.ActionId)
                    }
                })
            .Append(
                new[]
                {
                    new InlineKeyboardButton($"Закончить")
                    {
                        CallbackData = CommandsConstants.FinishCheckupRoutineActions
                    }
                });

        var inline = new InlineKeyboardMarkup(buttons);

        return new Message(text, inline);
    }

    public static Message FinishCheckup()
    {
        var text = $"Все записно!";
        var buttons = Array.Empty<InlineKeyboardButton>();
        var inline = new InlineKeyboardMarkup(buttons);

        return new Message(text, inline);
    }
}
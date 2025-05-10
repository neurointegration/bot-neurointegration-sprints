using BotTemplate.Models.Telegram;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotTemplate.Scenarios.User.Settings;

public static class ShowSettingsMessage
{
    private const string Text =
        "Настройки. Здесь ты можешь изменить какие-либо данные о себе или поменять время или дату";

    public static Message GetMessage(Neurointegration.Api.DataModels.Models.User user)
    {
        var changeIAmCoachButton =
            new[]
            {
                new InlineKeyboardButton(user.IAmCoach ? "Я больше не хочу быть тренером" : "Я хочу быть тренером")
                {
                    CallbackData = CommandsConstants.ChangeCoachStatus
                }
            };
        var changeSendRegularMessages =
            new[]
            {
                new InlineKeyboardButton(user.SendRegularMessages
                    ? "Я хочу остановить прохождение спринтов"
                    : "Я хочу проходить спринты")
                {
                    CallbackData = CommandsConstants.ChangeSprintRegular
                }
            };
        var changeEveningStandUpTime = new[]
        {
            new InlineKeyboardButton("Изменить время вечернего стендапа")
            {
                CallbackData = CommandsConstants.ChangeEveningStanUpTime
            }
        };
        var changeReflectionTime = new[]
        {
            new InlineKeyboardButton("Изменить время недельной рефлексии")
            {
                CallbackData = CommandsConstants.ChangeReflectionTime
            }
        };
        var changeMessageRangeTime = new[]
        {
            new InlineKeyboardButton("Изменить время отправки сообщений")
            {
                CallbackData = CommandsConstants.ChangeStatusTimeRange
            }
        };

        var buttons = new List<InlineKeyboardButton[]>
        {
            changeIAmCoachButton,
            changeSendRegularMessages,
            changeReflectionTime,
            changeEveningStandUpTime,
            changeMessageRangeTime
        };

        var inline = new InlineKeyboardMarkup(buttons);

        return new Message(Text, inline);
    }
}
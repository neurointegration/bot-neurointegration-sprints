using BotTemplate.Models.Telegram;
using Neurointegration.Api.DataModels.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotTemplate.Services.Telegram.Messages.Settings;

public static class ShowSettingsMessage
{
    private const string Text = "Настройки. Здесь ты можешь изменить какие-либо данные о себе или поменять время или дату";
        
        public static Message GetMessage(User user)
        {
            var changeIAmCoachButton =
                new[] {new InlineKeyboardButton(user.IAmCoach ? "Я больше не хочу быть тренером" : "Я хочу быть тренером")
                {
                    CallbackData = user.IAmCoach ? "Не тренер" : "Тренер"
                } };
            var changeSendRegularMessages =
                new[] { new InlineKeyboardButton(user.SendRegularMessages ? "Я больше не хочу проходить спринты" : "Я хочу проходить спринты")
                {
                    CallbackData = user.SendRegularMessages ? "Не проходить" : "Проходить"
                } };
            var changeEveningStandUpTime = new[] { new InlineKeyboardButton("Изменить время вечернего стендапа")
            {
                CallbackData = "Вечерний стендап"
            } };
            var changeMessageRangeTime = new[] { new InlineKeyboardButton("Изменить интервал ежедневных\nсообщений о состоянии")
            {
                CallbackData = "Состояние"
            } };
            var back = new[] { new InlineKeyboardButton("Отмена")
            {
                CallbackData = "Отмена"
            } };

            var buttons = new List<InlineKeyboardButton[]>
            {
                changeIAmCoachButton,
                changeSendRegularMessages,
                changeEveningStandUpTime,
                changeMessageRangeTime,
                back
            };

            var inline = new InlineKeyboardMarkup(buttons);
    
            return new Message(Text, inline);
        }
}
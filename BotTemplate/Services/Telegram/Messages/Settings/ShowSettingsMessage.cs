using BotTemplate.Models.Telegram;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotTemplate.Services.Telegram.Messages.Settings;

public static class ShowSettingsMessage
{
    private const string Text = "Настройки. Здесь ты можешь изменить какие-либо данные о себе или поменять время или дату";
        
        public static Message GetMessage(bool iAmCoach, bool sendRegularMessages)
        {
            // Buttons
            var changeEmailButton = new[] { new InlineKeyboardButton("Изменить почту") { CallbackData = "Почта" } };
            var changeIAmCoachButton =
                new[] {new InlineKeyboardButton(iAmCoach ? "Я больше не хочу быть тренером" : "Я хочу быть тренером")
                {
                    CallbackData = iAmCoach ? "Не тренер" : "Тренер"
                } };
            var changeSendRegularMessages =
                new[] { new InlineKeyboardButton(sendRegularMessages ? "Я больше не хочу проходить спринты" : "Я хочу проходить спринты")
                {
                    CallbackData = sendRegularMessages ? "Не проходить" : "Проходить"
                } };
            var changeEveningStandUpTime = new[] { new InlineKeyboardButton("Изменить время вечернего стендапа")
            {
                CallbackData = "Вечерний стендап"
            } };
            var changeMessageRangeTime = new[] { new InlineKeyboardButton("Изменить интервал ежедневных\nсообщений о состоянии")
            {
                CallbackData = "Состояние"
            } };
            var changeSprintStartDate = new[] { new InlineKeyboardButton("Изменить дату начала спринта")
            {
                CallbackData = "Начало спринта"
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
            
            // Keyboard markup
            var inline = new InlineKeyboardMarkup(buttons);
    
            return new Message(Text, inline);
        }
}
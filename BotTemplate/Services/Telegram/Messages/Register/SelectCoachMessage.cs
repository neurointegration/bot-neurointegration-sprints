using BotTemplate.Models.Telegram;
using Neurointegration.Api.DataModels.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotTemplate.Services.Telegram.Messages.Register;

public class SelectCoachMessage
{
    private const string Text = "Супер! И последний вопрос. Выбери своего тренера:";
    
    public static Message GetMessage(List<User> coaches)
    {
        // Buttons
        var buttons = coaches.Select(coach => new[] { new InlineKeyboardButton(coach.Username)
        {
            CallbackData = coach.UserId.ToString()
        } });
        
        // Keyboard markup
        var inline = new InlineKeyboardMarkup(buttons);

        return new Message(Text, inline);
    }
}
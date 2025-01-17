using BotTemplate.Models.Telegram;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotTemplate.Scenarios.RegularScenarios;

public static class EveningStandUpMessages
{
    public static Message AskReady(string scenarioToStartId)
    {
        var text = "Время заполнять стендап!";
        var readyButton = new InlineKeyboardButton($"Заполнить сейчас")
        {
            CallbackData = CommandsConstants.StartScenarioAction(scenarioToStartId)
        };

        var buttons = new InlineKeyboardMarkup(new[] 
        { 
            new[] { readyButton }
        });

        return new Message(text, buttons);
    }

    public static Message AskWinning()
    {
        var text = "Какие победы были сегодня?";

        return new Message(text);
    }
    
    public static Message AskLive()
    {
        var text = "Отлично! Какой лайф берешь на ближайшие сутки?";

        return new Message(text);
    }
    
    public static Message AskPleasure()
    {
        var text = "Что насчет кайфа?";

        return new Message(text);
    }
    
    public static Message AskDrive()
    {
        var text = "А как там с драйвом?";

        return new Message(text);
    }
    
    public static Message AskStartCheckUpRoutineActions()
    {
        var text = "Отметим выполненные привычки?";
        var startButton = new InlineKeyboardButton($"Да!")
        {
            CallbackData = CommandsConstants.StartCheckupRoutineActions
        };
        var finishButton = new InlineKeyboardButton($"Не сейчас")
        {
            CallbackData = CommandsConstants.FinishEveningStandUp
        };
        
        var buttons = new InlineKeyboardMarkup(new[] 
        { 
            new[] { startButton, finishButton }
        });

        return new Message(text, buttons);
    }
    
    public static Message Finish()
    {
        var text = "Успехов. До завтра!";

        return new Message(text);
    }
}
using BotTemplate.Models.Telegram;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotTemplate.Scenarios.RegularScenarios;

public static class WeekendReflectionMessages
{
    public static Message AskReady(string scenarioToStartId)
    {
        var text = "Время порефлексировать!";
        var readyButton = new InlineKeyboardButton($"Заполнить сейчас")
        {
            CallbackData = $"{CommandsConstants.StartReflectionActionPrefix} {scenarioToStartId}"
        };

        var buttons = new InlineKeyboardMarkup(new[] 
        { 
            new[] { readyButton }
        });

        return new Message(text, buttons);
    }

    public static Message AskWhatIDoing()
    {
        return new Message("Что сделал по всем проектам на этой неделе?");
    }

    public static Message AskWhatINotDoing()
    {
        return new Message("А что не сделал по всем проектам на этой неделе?");
    }
    
    public static Message AskStatus()
    {
        return new Message("Что влияло на твое состояние на этой неделе? Как в позитивном, так и в негативном ключе");
    }
    
    public static Message AskOrbits()
    {
        return new Message("Каким образом в поведении на этой неделе проявлялись орбиты? Как выруливал?");
    }
    
    public static Message AskCorrection()
    {
        return new Message("Что изменишь на следующей неделе?");
    }
    
    public static Message FinishReflection()
    {
        return new Message("Желаю успехов в новой неделе!");
    }
    
    public static Message AskChanges()
    {
        return new Message("Какое главное изменение замечаешь?");
    }

    public static Message AskActions()
    {
        return new Message("Какие твои действия к этому привели?");
    }
    
    public static Message AskAbilities()
    {
        return new Message("Какие твои способности мне помогли?");
    }
    
    public static Message AskBeliefs()
    {
        return new Message("Как изменились твои убеждения о том, что возможно?");
    }
    
    public static Message AskSelfPerception()
    {
        return new Message("Как изменились твои убеждения о себе и отношения с собой?");
    }
    
    public static Message AskChangeRoutineActions()
    {
        var text = "Надо внести изменения в список рутиннх действий?";
        var startButton = new InlineKeyboardButton($"Да!")
        {
            CallbackData = CommandsConstants.ChangeRoutineActions
        };
        var finishButton = new InlineKeyboardButton($"Нет")
        {
            CallbackData = CommandsConstants.FinishWeekendReflection
        };
        
        var buttons = new InlineKeyboardMarkup(new[] 
        { 
            new[] { startButton, finishButton }
        });

        return new Message(text, buttons);
    }
    
    public static Message AskOpportunities()
    {
        return new Message("Какие возможности теперь для тебя доступны?");
    }
}
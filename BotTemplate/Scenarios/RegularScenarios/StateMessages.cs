using BotTemplate.Models.Telegram;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotTemplate.Scenarios.RegularScenarios;

public static class StateMessages
{
    public static Message AskStatus(string scenarioToStartId)
    {
        var redButton = new InlineKeyboardButton($"\ud83d\udd25 {CommandsConstants.StatusPanic} \ud83d\udd25");
        var orangeButton = new InlineKeyboardButton($"\u26a1\ufe0f {CommandsConstants.StatusOverexcitation} \u26a1\ufe0f");
        var yellowButton = new InlineKeyboardButton($"\u2b50 {CommandsConstants.StatusInclusion} \u2b50\ufe0f");
        var greenButton = new InlineKeyboardButton($"\ud83c\udf40 {CommandsConstants.StatusBalance} \ud83c\udf40");
        var lightBlueButton = new InlineKeyboardButton($"\ud83e\udd8b {CommandsConstants.StatusRelaxation} \ud83e\udd8b");
        var blueButton = new InlineKeyboardButton($"\ud83d\ude45\ud83c\udffb\u200d\u2642\ufe0f {CommandsConstants.StatusPassivity} \ud83d\ude45\ud83c\udffb\u200d\u2642\ufe0f");
        var purpleButton = new InlineKeyboardButton($"\u2614\ufe0f {CommandsConstants.StatusApathy} \u2614");

        redButton.CallbackData = CommandsConstants.StartScenarioAction(scenarioToStartId, CommandsConstants.StatusPanic);
        orangeButton.CallbackData = CommandsConstants.StartScenarioAction(scenarioToStartId, CommandsConstants.StatusOverexcitation);
        yellowButton.CallbackData = CommandsConstants.StartScenarioAction(scenarioToStartId, CommandsConstants.StatusInclusion);
        greenButton.CallbackData = CommandsConstants.StartScenarioAction(scenarioToStartId, CommandsConstants.StatusBalance);
        lightBlueButton.CallbackData = CommandsConstants.StartScenarioAction(scenarioToStartId, CommandsConstants.StatusRelaxation);
        blueButton.CallbackData = CommandsConstants.StartScenarioAction(scenarioToStartId, CommandsConstants.StatusPassivity);
        purpleButton.CallbackData = CommandsConstants.StartScenarioAction(scenarioToStartId, CommandsConstants.StatusApathy);
    
        var buttons = new[] 
        { 
            new[] { redButton }, 
            new[] { orangeButton }, 
            new[] { yellowButton }, 
            new[] { greenButton },
            new[] { lightBlueButton },
            new[] { blueButton },
            new[] { purpleButton } 
        };
        
        var inline = new InlineKeyboardMarkup(buttons);

        var text = "Как ты себя сейчас чувствуешь?";
        return new Message(text, inline);
    }
    
    public static Message GetRecommendation(string answer)
    {
        string message;
        
        switch (answer)
        {
            case CommandsConstants.StatusPanic:
                message = "Замедли дыхание, приляг или сядь. Попробуй отвлечься, например, начни считать все синие предметы, которые видишь вокруг";
                break;
            case CommandsConstants.StatusOverexcitation:
                message = "Попробуй медленно и глубоко дышать. Если есть возможность, выполни небольшую физическую активность. Это поможет сбросить лишнюю энергию";
                break;
            case CommandsConstants.StatusInclusion:
                message = "Классно, что ты погружен в дело! Не забывай, что чрезмерная включенность в работу может привести к переутомлению";
                break;
            case CommandsConstants.StatusBalance:
                message = "Отлично! Продолжай в том же духе! Хорошего тебе дня!";
                break;
            case CommandsConstants.StatusRelaxation:
                message = "Если расслабленность мешает твоей продуктивности, то попробуй спланировать день/неделю.  Это помогает сосредоточиться на целях и побудить к действию";
                break;
            case CommandsConstants.StatusPassivity:
                message = "Не требуй слишком много от себя. Иногда важно дать отдохнуть и позволить себе не делать ничего";
                break;
            case CommandsConstants.StatusApathy:
                message = "Дай себе возможность отдохнуть. Лучше поспать или выпить некрепкий сладкий чай";
                break;
            default:
                message = "Я пока не знаю, что это за состояние";
                break;
        }

        return new Message(message);
    }
}
using BotTemplate.Models.ScenariosData;
using BotTemplate.Models.Telegram;

namespace BotTemplate.Extensions;

public static class ScenarioToStartExtensions
{
    public static CurrentScenarioInfo ToCurrentScenarioInfo(this ScenarioToStart scenarioToStart)
    {
        return new CurrentScenarioInfo
        {
            ChatId = scenarioToStart.ChatId,
            ScenarioId = scenarioToStart.ScenarioId,
            CurrentSprintNumber = scenarioToStart.SprintNumber,
            SprintReplyNumber = scenarioToStart.SprintReplyNumber,
            Index = 0,
            Date = scenarioToStart.Date,
            Data = scenarioToStart.Data
        };
    }
}
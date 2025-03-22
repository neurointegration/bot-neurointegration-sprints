using BotTemplate.Models.ScenariosData;
using BotTemplate.Models.Telegram;
using Neurointegration.Api.DataModels.Models;

namespace BotTemplate.Extensions;

public static class ScenarioToStartExtensions
{
    public static Question ToQuestion(this ScenarioToStart scenarioToStart)
    {
        return new Question(
            scenarioToStart.Date ?? DateTime.UtcNow,
            scenarioToStart.ChatId,
            scenarioToStart.ScenarioType,
            scenarioToStart.SprintNumber,
            scenarioToStart.SprintReplyNumber,
            scenarioToStart.Priority,
            scenarioToStart.IsDelayed
        );
    }
}
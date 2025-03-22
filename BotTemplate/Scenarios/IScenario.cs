using BotTemplate.Models.Telegram;

namespace BotTemplate.Scenarios;

public interface IScenario
{
    Task<bool> TryHandle(TelegramEvent telegramEvent, CurrentScenarioInfo? scenarioInfo);
}
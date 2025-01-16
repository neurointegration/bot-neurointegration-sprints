
using BotTemplate.Models.Telegram;
using Neurointegration.Api.DataModels.Models;

namespace BotTemplate.Scenarios.RegularScenarios;

public interface IRegularScenario : IScenario
{
    Task<bool> TryAddToStart(Question question);
    Task<bool> TryStart(Question question);
}
using BotTemplate.Client;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.YDB;
using Microsoft.Extensions.Logging;
using Neurointegration.Api.DataModels.Models;

namespace BotTemplate.Scenarios.RegularScenarios;

public class EveningStandUpScenario: BaseRegularScenario
{
    protected override string ScenarioId => "regular_evening_standup";
    public override ScenarioType ScenarioType => ScenarioType.EveningStandUp;

    public EveningStandUpScenario(
        ScenarioStateRepository scenarioStateRepository,
        IMessageSender messageSender,
        IBackendApiClient backendApiClient,
        ILogger logger) : base(scenarioStateRepository, messageSender, backendApiClient, logger)
    {
    }
}
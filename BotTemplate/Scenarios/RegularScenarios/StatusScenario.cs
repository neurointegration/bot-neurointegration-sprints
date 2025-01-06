using BotTemplate.Client;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.YDB;
using Microsoft.Extensions.Logging;
using Neurointegration.Api.DataModels.Models;

namespace BotTemplate.Scenarios.RegularScenarios;

public class StatusScenario: BaseRegularScenario
{
    protected override string ScenarioId => "regular_status";
    public override ScenarioType ScenarioType => ScenarioType.Status;

    public StatusScenario(
        ScenarioStateRepository scenarioStateRepository,
        IMessageSender messageSender,
        IBackendApiClient backendApiClient,
        ILogger logger) : base(scenarioStateRepository, messageSender, backendApiClient, logger)
    {
    }
}
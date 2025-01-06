using BotTemplate.Client;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.YDB;
using Microsoft.Extensions.Logging;
using Neurointegration.Api.DataModels.Models;

namespace BotTemplate.Scenarios.RegularScenarios;

public class WeekendReflectionScenario : BaseRegularScenario
{
    protected override string ScenarioId => "regular_weekend_reflection";
    private static string LastReflectionScenarioId => "last_regular_weekend_reflection";
    public override ScenarioType ScenarioType => ScenarioType.Reflection;
    public const int SprintReplyCount = 4;


    public WeekendReflectionScenario(
        ScenarioStateRepository scenarioStateRepository,
        IMessageSender messageSender,
        IBackendApiClient backendApiClient,
        ILogger logger) : base(scenarioStateRepository, messageSender, backendApiClient, logger)
    {
    }

    public override async Task Start(Question question)
    {
        string? message;
        if (question.SprintReplyNumber == SprintReplyCount - 1)
        {
            Logger.LogInformation($"Начало сценария `последняя рефлексия` для пользователя {question.UserId}");
            message = await ScenarioStateRepository.StartNewScenarioAndGetMessage(question.UserId,
                LastReflectionScenarioId,
                question.Date, question.SprintNumber, question.SprintReplyNumber);
        }
        else
        {
            Logger.LogInformation($"Начало сценария `обычная рефлексия` для пользователя {question.UserId}");
            message = await ScenarioStateRepository.StartNewScenarioAndGetMessage(question.UserId, ScenarioId,
                question.Date, question.SprintNumber, question.SprintReplyNumber);
        }

        await MessageSender.TrySay(message, question.UserId);
    }
}
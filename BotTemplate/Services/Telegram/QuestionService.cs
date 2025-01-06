using BotTemplate.Client;
using BotTemplate.Models;
using BotTemplate.Scenarios.RegularScenarios;
using BotTemplate.Services.YDB;
using Microsoft.Extensions.Logging;
using Neurointegration.Api.DataModels.Models;

namespace BotTemplate.Services.Telegram;

public class QuestionService
{
    private readonly IBackendApiClient backendApiClient;
    private readonly ScenarioStateRepository scenarioStateRepository;
    private readonly StatusScenario statusScenario;
    private readonly EveningStandUpScenario eveningStandUpScenario;
    private readonly WeekendReflectionScenario weekendReflectionScenario;
    private readonly ILogger logger;

    private const int DefaultRequestTimeMinutes = 2;

    public QuestionService(
        IBackendApiClient backendApiClient,
        ScenarioStateRepository scenarioStateRepository,
        StatusScenario statusScenario,
        EveningStandUpScenario eveningStandUpScenario,
        WeekendReflectionScenario weekendReflectionScenario,
        ILogger logger)
    {
        this.backendApiClient = backendApiClient;
        this.scenarioStateRepository = scenarioStateRepository;
        this.statusScenario = statusScenario;
        this.eveningStandUpScenario = eveningStandUpScenario;
        this.weekendReflectionScenario = weekendReflectionScenario;
        this.logger = logger;
    }

    public async Task<int> AskQuestions(QuestionRequest questionRequest)
    {
        var questions = await backendApiClient.GetQuestionsAsync(
            questionRequest.Time ?? DefaultRequestTimeMinutes,
            questionRequest.ScenarioType);

        foreach (var question in questions)
        {
            await scenarioStateRepository.EndScenarioNoMatterWhat(question.UserId);

            switch (question.ScenarioType)
            {
                case ScenarioType.Status:
                    await statusScenario.Start(question);
                    continue;
                case ScenarioType.EveningStandUp:
                    await eveningStandUpScenario.Start(question);
                    continue;
                case ScenarioType.Reflection:
                    await weekendReflectionScenario.Start(question);
                    continue;
                default:
                    logger.LogWarning($"Неизвестный тип вопроса {question.ScenarioType} для пользователя {question.UserId}");
                    continue;
            }
        }

        return questions.Count;
    }
}
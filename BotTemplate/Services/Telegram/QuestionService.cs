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
    private readonly IEnumerable<IRegularScenario> regularScenarios;
    private readonly ILogger logger;

    private const int DefaultRequestTimeMinutes = 2;

    public QuestionService(
        IBackendApiClient backendApiClient,
        ScenarioStateRepository scenarioStateRepository,
        IEnumerable<IRegularScenario> regularScenarios,
        ILogger logger)
    {
        this.backendApiClient = backendApiClient;
        this.scenarioStateRepository = scenarioStateRepository;
        this.regularScenarios = regularScenarios;
        this.logger = logger;
    }

    public async Task<int> AskQuestions(QuestionRequest questionRequest)
    {
        var questions = await backendApiClient.GetQuestionsAsync(
            questionRequest.Time ?? DefaultRequestTimeMinutes,
            questionRequest.ScenarioType);

        foreach (var question in questions)
        {
            var success = false;

            foreach (var scenario in regularScenarios)
            {
                success = await scenario.TryAddToStart(question);
                if (success)
                    break;
            }
            
            if (!success)
                logger.LogWarning($"Неизвестный тип вопроса {question.ScenarioType} для пользователя {question.UserId}");
        }

        return questions.Count;
    }
}
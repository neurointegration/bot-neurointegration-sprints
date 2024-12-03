using BotTemplate.Client;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.Telegram.MessageCommands;
using BotTemplate.Services.YDB;
using BotTemplate.Services.YDB.Repo;
using Microsoft.Extensions.Logging;
using Neurointegration.Api.DataModels.Models;

namespace BotTemplate.Models.Telegram;

public class QuestionService
{
    private readonly IMessageView messageView;
    private readonly IBotDatabase botDatabase;
    private readonly IBackendApiClient backendApiClient;
    private readonly ILogger logger;

    private const int DefaultRequestTimeMinutes = 2;


    public QuestionService(
        IMessageView messageView,
        IBotDatabase botDatabase,
        IBackendApiClient backendApiClient,
        ILogger logger)
    {
        this.messageView = messageView;
        this.botDatabase = botDatabase;
        this.backendApiClient = backendApiClient;
        this.logger = logger;
    }

    public async Task<int> AskQuestions(QuestionRequest questionRequest)
    {
        var scenariosRepository = await ScenariosRepository.Init(botDatabase);
        var currentScenarioRepository = await CurrentScenarioRepository.Init(botDatabase, scenariosRepository);
        await UserAnswersRepository.Init(botDatabase);
        await UsersRepository.Init(botDatabase);

        var messageCommands = new IMessageCommand[]
        {
            new SendStateMessage(),
            new HandleStateResponse()
        };

        var questions = await backendApiClient.GetQuestionsAsync(questionRequest.Time ?? DefaultRequestTimeMinutes, questionRequest.ScenarioType);

        foreach (var question in questions)
        {
            await currentScenarioRepository.EndScenarioNoMatterWhat(question.UserId);
            string? message = null;

            switch (question.ScenarioType)
            {
                case ScenarioType.Status:
                    logger.LogInformation("Start status scenario");
                    message = await currentScenarioRepository.StartNewScenarioAndGetMessage(question.UserId, 1,
                        question.Date, question.SprintNumber, question.SprintReplyNumber);
                    break;
                case ScenarioType.EveningStandUp:
                    logger.LogInformation("Start evening stand up scenario");
                    message = await currentScenarioRepository.StartNewScenarioAndGetMessage(question.UserId, 2,
                        question.Date, question.SprintNumber, question.SprintReplyNumber);
                    break;
                case ScenarioType.Reflection:
                    if (question.SprintReplyNumber == 3)
                    {
                        logger.LogInformation("Start final reflection scenario");
                        message = await currentScenarioRepository.StartNewScenarioAndGetMessage(question.UserId, 4,
                            question.Date, question.SprintNumber, question.SprintReplyNumber);
                    }
                    else
                    {
                        logger.LogInformation("Start ordinar reflection scenario");
                        message = await currentScenarioRepository.StartNewScenarioAndGetMessage(question.UserId, 3,
                            question.Date, question.SprintNumber, question.SprintReplyNumber);
                    }
                    break;
            }

            if (message != null && message.StartsWith('/'))
            {
                var messageCommand =
                    messageCommands.FirstOrDefault(messageCommand => message.StartsWith(messageCommand.Command));
                await messageCommand!.Handle(messageView, question.UserId, null);
                continue;
            }

            await messageView.Say(message!, question.UserId);
        }

        return questions.Count;
    }
}
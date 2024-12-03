using BotTemplate.Client;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.Telegram.MessageCommands;
using BotTemplate.Services.YDB;
using BotTemplate.Services.YDB.Repo;
using Neurointegration.Api.DataModels.Models;

namespace BotTemplate.Models.Telegram;

public class QuestionService
{
    private readonly IMessageView messageView;
    private readonly IBotDatabase botDatabase;
    private readonly IBackendApiClient backendApiClient;
    private readonly int triggerFrequencyMinutes;
    

    public QuestionService(
        IMessageView messageView,
        IBotDatabase botDatabase,
        IBackendApiClient backendApiClient,
        int triggerFrequencyMinutes)
    {
        this.messageView = messageView;
        this.botDatabase = botDatabase;
        this.backendApiClient = backendApiClient;
        this.triggerFrequencyMinutes = triggerFrequencyMinutes;
    }

    public async Task<int> AskQuestions()
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
        
        var questions = await backendApiClient.GetQuestionsAsync(triggerFrequencyMinutes);

        foreach (var question in questions)
        {
            await currentScenarioRepository.EndScenarioNoMatterWhat(question.UserId);
            string? message = null;

            switch (question.ScenarioType)
            {
                case ScenarioType.Status:
                    message = await currentScenarioRepository.StartNewScenarioAndGetMessage(question.UserId, 1,
                        question.Date, question.SprintNumber, question.SprintReplyNumber);
                    break;
                case ScenarioType.EveningStandUp:
                    message = await currentScenarioRepository.StartNewScenarioAndGetMessage(question.UserId, 2,
                        question.Date, question.SprintNumber, question.SprintReplyNumber);
                    break;
                case ScenarioType.Reflection:
                    if (question.SprintReplyNumber == 3)
                        message = await currentScenarioRepository.StartNewScenarioAndGetMessage(question.UserId, 4,
                            question.Date, question.SprintNumber, question.SprintReplyNumber);
                    else
                        message = await currentScenarioRepository.StartNewScenarioAndGetMessage(question.UserId, 3,
                            question.Date, question.SprintNumber, question.SprintReplyNumber);
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
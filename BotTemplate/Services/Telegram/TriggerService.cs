using BotTemplate.Client;
using BotTemplate.Models.ClientDto;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.Telegram.Commands;
using BotTemplate.Services.Telegram.MessageCommands;
using BotTemplate.Services.YDB;
using BotTemplate.Services.YDB.Repo;
using Telegram.Bot.Types;

namespace BotTemplate.Models.Telegram;

public class TriggerService
{
    private readonly IMessageView _messageView;
    private readonly IBotDatabase _botDatabase;
    private readonly IBackendApiClient _backendApiClient;
    private readonly string _telegramBotUrl;
    private readonly HttpClient _client = new();
    private readonly int _triggerFrequencyMinutes;
    
    private IMessageCommand[] _messageCommands;
    
    private CurrentScenarioRepo _currentScenarioRepo;
    private UserAnswersRepo _userAnswersRepo;
    private UsersRepo _usersRepo;

    public TriggerService(
        IMessageView messageView,
        IBotDatabase botDatabase,
        IBackendApiClient backendApiClient,
        string telegramBotUrl,
        int triggerFrequencyMinutes)
    {
        _messageView = messageView;
        _botDatabase = botDatabase;
        _backendApiClient = backendApiClient;
        _telegramBotUrl = telegramBotUrl;
        _triggerFrequencyMinutes = triggerFrequencyMinutes;
    }

    public async Task Handle()
    {
        var scenariosRepo = await ScenariosRepo.InitWithDatabase(_botDatabase);
        _currentScenarioRepo = await CurrentScenarioRepo.InitWithDatabase(_botDatabase, scenariosRepo);
        _userAnswersRepo = await UserAnswersRepo.InitWithDatabase(_botDatabase);
        _usersRepo = await UsersRepo.InitWithDatabase(_botDatabase);

        _messageCommands = new IMessageCommand[]
        {
            new SendStateMessage(),
            new HandleStateResponse()
        };
        
        var updates = await _backendApiClient.GetQuestionsAsync(_triggerFrequencyMinutes);

        if (updates is null)
            return;

        foreach (var update in updates)
        {
            await _currentScenarioRepo.EndScenarioNoMatterWhat(update.UserId);
            string? message = null;

            switch (update.ScenarioType)
            {
                case ScenarioType.Status:
                    message = await _currentScenarioRepo.StartNewScenarioAndGetMessage(update.UserId, 1,
                        update.SprintNumber, update.SprintReplyNumber);
                    break;
                case ScenarioType.EveningStandUp:
                    message = await _currentScenarioRepo.StartNewScenarioAndGetMessage(update.UserId, 2,
                        update.SprintNumber, update.SprintReplyNumber);
                    break;
                case ScenarioType.Reflection:
                    if (update.SprintReplyNumber == 3)
                        message = await _currentScenarioRepo.StartNewScenarioAndGetMessage(update.UserId, 4,
                            update.SprintNumber, update.SprintReplyNumber);
                    else
                        message = await _currentScenarioRepo.StartNewScenarioAndGetMessage(update.UserId, 3,
                            update.SprintNumber, update.SprintReplyNumber);
                    break;
            }

            if (message != null && message.StartsWith('/'))
            {
                var messageCommand =
                    _messageCommands.FirstOrDefault(messageCommand => message.StartsWith(messageCommand.Command));
                await messageCommand!.Handle(_messageView, update.UserId, null);
                return;
            }

            await _messageView.Say(message!, update.UserId);
        }
    }
}
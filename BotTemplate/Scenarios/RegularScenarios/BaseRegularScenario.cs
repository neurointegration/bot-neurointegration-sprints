using BotTemplate.Client;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.Telegram.Messages.Status;
using BotTemplate.Services.YDB;
using Microsoft.Extensions.Logging;
using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Models;

namespace BotTemplate.Scenarios.RegularScenarios;

public abstract class BaseRegularScenario : IRegularScenario
{
    protected readonly ScenarioStateRepository ScenarioStateRepository;
    protected readonly ILogger Logger;
    protected readonly IMessageSender MessageSender;
    protected readonly IBackendApiClient BackendApiClient;
    private HashSet<IMessageCommand> messageCommands;
    protected virtual string ScenarioId { get; } = default!;
    public virtual ScenarioType ScenarioType { get; } = default!;

    public BaseRegularScenario(
        ScenarioStateRepository scenarioStateRepository,
        IMessageSender messageSender,
        IBackendApiClient backendApiClient,
        ILogger logger)
    {
        this.ScenarioStateRepository = scenarioStateRepository;
        this.Logger = logger;
        this.MessageSender = messageSender;
        this.BackendApiClient = backendApiClient;
        messageCommands = new HashSet<IMessageCommand>()
        {
            new HandleStateResponse(),
            new SendStateMessage()
        };
    }

    public virtual async Task Start(Question question)
    {
        Logger.LogInformation($"Начало сценария `{ScenarioType.ToString()}` для пользователя {question.UserId}");
        var message = await ScenarioStateRepository.StartNewScenarioAndGetMessage(question.UserId, ScenarioId,
            question.Date, question.SprintNumber, question.SprintReplyNumber);
        
        if (!await TryProcessCommand(message, question.UserId))
            await MessageSender.TrySay(message, question.UserId);
    }

    public virtual async Task Handle(TelegramEvent telegramEvent)
    {
        var chatId = telegramEvent.ChatId;
        var currentScenarioInfo = await ScenarioStateRepository.GetInfoByChatId(chatId);

        var sendAnswer = new SendAnswer
        {
            UserId = chatId,
            Answer = telegramEvent.Text ?? "",
            AnswerNumber = currentScenarioInfo.Index!.Value,
            ScenarioType = ScenarioType,
            Date = DateOnly.FromDateTime(currentScenarioInfo.Date ?? DateTime.UtcNow),
            SprintNumber = (int) currentScenarioInfo.CurrentSprintNumber!.Value,
            SprintReplyNumber = currentScenarioInfo.SprintReplyNumber!.Value
        };
        await BackendApiClient.SendAnswerAsync(sendAnswer);

        var message = await ScenarioStateRepository.IncreaseAndGetNewMessage(chatId);
        if (!await TryProcessCommand(message, chatId))
            await MessageSender.TrySay(message, chatId);
        
        await ScenarioStateRepository.TryEndScenario(chatId);
    }

    private async Task<bool> TryProcessCommand(string? message, long userId)
    {
        if (message != null && message.StartsWith('/'))
        {
            var messageCommand =
                messageCommands.FirstOrDefault(messageCommand => message.StartsWith(messageCommand.Command));
            await messageCommand!.Handle(MessageSender, userId, null);
            return true;
        }

        return false;
    }
}
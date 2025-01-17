using BotTemplate.Client;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.Telegram.Messages.Settings;
using BotTemplate.Services.Telegram.Validators;
using BotTemplate.Services.YDB;
using Neurointegration.Api.DataModels.Dto;

namespace BotTemplate.Scenarios.Common;

public class ChangeStateTimeRangeScenario : IScenario
{
    private readonly IBackendApiClient backendApiClient;
    private readonly IMessageSender messageSender;
    private readonly ScenarioStateRepository scenarioStateRepository;
    private const string ScenarioId = CommandsConstants.ChangeStatusTimeRange;
    private const string Command = CommandsConstants.ChangeStatusTimeRange;

    public ChangeStateTimeRangeScenario(
        IBackendApiClient backendApiClient,
        IMessageSender messageSender,
        ScenarioStateRepository scenarioStateRepository)
    {
        this.backendApiClient = backendApiClient;
        this.messageSender = messageSender;
        this.scenarioStateRepository = scenarioStateRepository;
    }

    public async Task<bool> TryHandle(TelegramEvent telegramEvent, CurrentScenarioInfo? scenarioInfo)
    {
        var text = telegramEvent.Text?.Trim().ToLower();
        if (text != Command && scenarioInfo?.ScenarioId != ScenarioId)
            return false;

        var chatId = telegramEvent.ChatId;
        if (text == Command)
        {
            await messageSender.TrySay(ChangeStatusTimeMessage.GetMessage(), chatId);
            await scenarioStateRepository.StartNewScenario(chatId, ScenarioId);
            return true;
        }
        
        var timeRangeValidator = new TimeRangeValidator();
        if (!timeRangeValidator.IsValid(text))
        {
            await messageSender.Say("Неправильный формат. Нужно ввести интервал по МСК в формете 9:00-18:00",
                chatId);
        }
        else
        {
            var timeSpanStrings = text!.Split('-');

            var mskStartTime = TimeSpan.Parse(timeSpanStrings[0]);
            var messageStartTime = mskStartTime.Subtract(TimeSpan.FromHours(3));
            if (messageStartTime < TimeSpan.FromHours(0))
                messageStartTime = messageStartTime.Add(TimeSpan.FromHours(24));

            var mskEndTime = TimeSpan.Parse(timeSpanStrings[1]);
            var messageEndTime = mskEndTime - TimeSpan.FromHours(3);
            if (messageEndTime < TimeSpan.FromHours(0))
                messageEndTime = messageEndTime.Add(TimeSpan.FromHours(24));

            var updateUser = new UpdateUser
            {
                UserId = chatId,
                MessageStartTime = messageStartTime,
                MessageEndTime = messageEndTime
            };
            await scenarioStateRepository.EndScenarioNoMatterWhat(chatId);
            await backendApiClient.UpdateUser(updateUser);
            await messageSender.Say($"Теперь я буду присылать уведомления о состоянии в {mskStartTime:hh.mm}-{mskEndTime:hh.mm}", chatId);
        }

        return true;
    }
}
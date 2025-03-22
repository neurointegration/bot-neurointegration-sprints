using BotTemplate.Client;
using BotTemplate.Models.Telegram;
using BotTemplate.Scenarios.Common.Messages;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.Telegram.Validators;
using BotTemplate.Services.YDB;
using Neurointegration.Api.DataModels.Dto;

namespace BotTemplate.Scenarios.Common;

public class ChangeWeekendReflectionTimeScenario : IScenario
{
    private readonly IBackendApiClient backendApiClient;
    private readonly IMessageSender messageSender;
    private readonly ScenarioStateRepository scenarioStateRepository;
    private const string ScenarioId = CommandsConstants.ChangeReflectionTime;
    private const string Command = CommandsConstants.ChangeReflectionTime;

    public ChangeWeekendReflectionTimeScenario(
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
        var chatId = telegramEvent.ChatId;

        if (text == Command)
        {
            await messageSender.TrySay(ChangeReflectionTimeMessage.GetMessage(), chatId);
            await scenarioStateRepository.StartNewScenario(chatId, ScenarioId);
            return true;
        }

        if (scenarioInfo?.ScenarioId != ScenarioId)
            return false;

        var timeValidator = new TimeValidator();
        if (!timeValidator.IsValid(text))
        {
            await messageSender.Say("Неправильный формат. Нужно ввести время по МСК в формете 9:00", chatId);
        }
        else
        {
            var mskWeekReflectionTime = TimeSpan.Parse(text!);
            var weekReflectionTime = mskWeekReflectionTime.Subtract(TimeSpan.FromHours(3));
            if (weekReflectionTime < TimeSpan.FromHours(0))
                weekReflectionTime = weekReflectionTime.Add(TimeSpan.FromHours(24));

            var updateUser = new UpdateUser
            {
                UserId = chatId,
                WeekReflectionTime = weekReflectionTime
            };
            await scenarioStateRepository.EndScenarioNoMatterWhat(chatId);
            await backendApiClient.UpdateUser(updateUser);
            await messageSender.Say(
                $"Теперь я буду присылать напоминание о еженедельной рефлексии в {mskWeekReflectionTime:hh\\:mm} по МСК", chatId);
        }

        return true;
    }
}
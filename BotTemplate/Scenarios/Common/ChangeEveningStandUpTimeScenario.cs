using BotTemplate.Client;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.Telegram.Messages.Settings;
using BotTemplate.Services.Telegram.Validators;
using BotTemplate.Services.YDB;
using Neurointegration.Api.DataModels.Dto;

namespace BotTemplate.Scenarios.Common;

public class ChangeEveningStandUpTimeScenario : IScenario
{
    private readonly IBackendApiClient backendApiClient;
    private readonly IMessageSender messageSender;
    private readonly ScenarioStateRepository scenarioStateRepository;
    private const string ScenarioId = CommandsConstants.ChangeEveningStanUpTime;
    private const string Command = CommandsConstants.ChangeEveningStanUpTime;

    public ChangeEveningStandUpTimeScenario(
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
            await messageSender.TrySay(ChangeEveningStandUpTimeMessage.GetMessage(), chatId);
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
            var eveningStandUpTime = TimeSpan.Parse(text!).Subtract(TimeSpan.FromHours(3));
            if (eveningStandUpTime < TimeSpan.FromHours(0))
                eveningStandUpTime = eveningStandUpTime.Add(TimeSpan.FromHours(24));

            var updateUser = new UpdateUser
            {
                UserId = chatId,
                EveningStandUpTime = eveningStandUpTime
            };
            await scenarioStateRepository.EndScenarioNoMatterWhat(chatId);
            await backendApiClient.UpdateUser(updateUser);
            await messageSender.Say("Время вечернего стендапа изменено.", chatId);
        }

        return true;
    }
}
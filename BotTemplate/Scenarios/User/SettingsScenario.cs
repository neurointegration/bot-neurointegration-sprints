using System.Globalization;
using BotTemplate.Client;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.Telegram.Messages.Bottom;
using BotTemplate.Services.Telegram.Messages.Settings;
using BotTemplate.Services.Telegram.Validators;
using BotTemplate.Services.YDB;
using Neurointegration.Api.DataModels.Dto;

namespace BotTemplate.Scenarios.User;

public class SettingsScenario
{
    private readonly ScenarioStateRepository scenarioStateRepository;
    private readonly IBackendApiClient backendApiClient;
    private readonly IMessageSender messageSender;
    public string ScenarioId => "change_settings";

    public SettingsScenario(
        ScenarioStateRepository scenarioStateRepository,
        IBackendApiClient backendApiClient,
        IMessageSender messageSender)
    {
        this.scenarioStateRepository = scenarioStateRepository;
        this.backendApiClient = backendApiClient;
        this.messageSender = messageSender;
    }

    public async Task Handle(TelegramEvent telegramEvent)
    {
        var text = telegramEvent.Text;
        var chatId = telegramEvent.ChatId;
        var index = await scenarioStateRepository.GetIndexByChatId(chatId);
        if (index is null)
        {
            await scenarioStateRepository.StartNewScenarioAndGetMessage(chatId, ScenarioId);
            index = -1;
        }
        else
            await scenarioStateRepository.IncreaseAndGetNewMessage(chatId);

        index++;
        var getUser = await backendApiClient.GetUser(chatId);
        if (!getUser.IsSuccess)
            await messageSender.Say(MessageConstants.UnknownErrorText, chatId);

        switch (index!.Value)
        {
            case 0:
                var message = ShowSettingsMessage.GetMessage(getUser.Value);
                await messageSender.SayWithMarkup(message.Text, chatId, message.ReplyMarkup);
                break;
            case 1:
                switch (text)
                {
                    case "Почта":
                        await messageSender.Say(ChangeEmailMessage.GetMessage().Text, chatId);
                        break;
                    case "Не тренер":
                        await scenarioStateRepository.EndScenarioNoMatterWhat(chatId);
                        await backendApiClient.UpdateUserAsync(new UpdateUser {UserId = chatId, IAmCoach = false});
                        await messageSender.SayWithMarkup("Теперь ты не тренер", chatId,
                            BottomMessage.GetMessage().ReplyMarkup);
                        break;
                    case "Тренер":
                        await scenarioStateRepository.EndScenarioNoMatterWhat(chatId);
                        await backendApiClient.UpdateUserAsync(new UpdateUser {UserId = chatId, IAmCoach = true});
                        await messageSender.SayWithMarkup("Теперь ты тренер", chatId,
                            BottomMessage.GetMessage(true).ReplyMarkup);
                        break;
                    case "Не проходить":
                        await scenarioStateRepository.EndScenarioNoMatterWhat(chatId);
                        await backendApiClient.UpdateUserAsync(new UpdateUser
                            {UserId = chatId, SendRegularMessages = false});
                        await messageSender.Say("Теперь ты больше не проходишь спринты", chatId);
                        break;
                    case "Проходить":
                        await scenarioStateRepository.EndScenarioNoMatterWhat(chatId);
                        await backendApiClient.UpdateUserAsync(new UpdateUser
                            {UserId = chatId, SendRegularMessages = true});
                        await messageSender.Say(
                            "Теперь ты проходишь спринты. Обязательно укажи в настройках остальные данные для спринтов",
                            chatId);
                        break;
                    case "Вечерний стендап":
                        await messageSender.Say(ChangeEveningStandUpTimeMessage.GetMessage().Text, chatId);
                        for (var i = 0; i < 1; i++)
                            await scenarioStateRepository.IncreaseAndGetNewMessage(chatId);
                        break;
                    case "Состояние":
                        await messageSender.Say(ChangeStatusTimeMessage.GetMessage().Text, chatId);
                        for (var i = 0; i < 2; i++)
                            await scenarioStateRepository.IncreaseAndGetNewMessage(chatId);
                        break;
                    case "Начало спринта":
                        await messageSender.Say(ChangeCurrentStandUpDateMessage.GetMessage().Text, chatId);
                        for (var i = 0; i < 3; i++)
                            await scenarioStateRepository.IncreaseAndGetNewMessage(chatId);
                        break;
                    case "Отмена":
                        await scenarioStateRepository.EndScenarioNoMatterWhat(chatId);
                        break;
                }

                break;
            case 2: // изменить почту
                var emailUpdateUser = new UpdateUser
                {
                    UserId = chatId
                };
                var emailValidator = new EmailValidator();
                if (!emailValidator.IsValid(text))
                {
                    await messageSender.Say("Неправильный формат почты.", chatId);
                    await scenarioStateRepository.DecreaseIndex(chatId);
                }
                else
                {
                    emailUpdateUser.Email = text;
                    await scenarioStateRepository.EndScenarioNoMatterWhat(chatId);
                    await messageSender.Say("Почта изменена.", chatId);
                }

                break;
            case 3: // изменить время веч. стендапа
                var eveningStandUpUpdateUser = new UpdateUser
                {
                    UserId = chatId
                };
                var timeValidator = new TimeValidator();
                if (!timeValidator.IsValid(text))
                {
                    await messageSender.Say("Неправильный формат. Нужно ввести время по МСК в формете 9:00", chatId);
                    await scenarioStateRepository.DecreaseIndex(chatId);
                }
                else
                {
                    var trimmedText = text!.Trim();

                    var eveningStandUpTime = TimeSpan.Parse(trimmedText).Subtract(TimeSpan.FromHours(3));
                    if (eveningStandUpTime < TimeSpan.FromHours(0))
                        eveningStandUpTime = eveningStandUpTime.Add(TimeSpan.FromHours(24));

                    eveningStandUpUpdateUser.EveningStandUpTime = eveningStandUpTime;
                    await scenarioStateRepository.EndScenarioNoMatterWhat(chatId);
                    await backendApiClient.UpdateUserAsync(eveningStandUpUpdateUser);
                    await messageSender.Say("Время вечернего стендапа изменено.", chatId);
                }

                break;
            case 4: // изменить интервал
                var timeRangeValidator = new TimeRangeValidator();
                var timeRangeUpdateUser = new UpdateUser
                {
                    UserId = chatId
                };
                if (!timeRangeValidator.IsValid(text))
                {
                    await messageSender.Say("Неправильный формат. Нужно ввести интервал по МСК в формете 9:00-18:00",
                        chatId);
                    await scenarioStateRepository.DecreaseIndex(chatId);
                }
                else
                {
                    var trimmedText = text!.Trim();
                    var timeSpanStrings = trimmedText.Split('-');

                    var messageStartTime = TimeSpan.Parse(timeSpanStrings[0]).Subtract(TimeSpan.FromHours(3));
                    if (messageStartTime < TimeSpan.FromHours(0))
                        messageStartTime = messageStartTime.Add(TimeSpan.FromHours(24));

                    var messageEndTime = TimeSpan.Parse(timeSpanStrings[1]) - TimeSpan.FromHours(3);
                    if (messageEndTime < TimeSpan.FromHours(0))
                        messageEndTime = messageEndTime.Add(TimeSpan.FromHours(24));

                    timeRangeUpdateUser.MessageStartTime = messageStartTime;
                    timeRangeUpdateUser.MessageEndTime = messageEndTime;

                    await scenarioStateRepository.EndScenarioNoMatterWhat(chatId);
                    await backendApiClient.UpdateUserAsync(timeRangeUpdateUser);
                    await messageSender.Say("Интервал опроса состояний изменен.", chatId);
                }

                break;
            case 5: // изменить дату начала спринта
                var sprintDateUpdateUser = new UpdateUser
                {
                    UserId = chatId
                };
                var dateValidator = new DateValidator();
                if (!dateValidator.IsValid(text))
                {
                    await messageSender.Say("Неправильный формат. Нужно ввести дату в формете 06.12.2024", chatId);
                    await scenarioStateRepository.DecreaseIndex(chatId);
                }
                else
                {
                    var sprintStartDate = DateTime.ParseExact(text.Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture,
                        DateTimeStyles.None);
                    sprintDateUpdateUser.SprintStartDate = sprintStartDate;

                    var firstReflectionDate = sprintStartDate.Add(TimeSpan.FromDays(6));
                    sprintDateUpdateUser.ReflectionDate = firstReflectionDate;

                    await scenarioStateRepository.EndScenarioNoMatterWhat(chatId);
                    await messageSender.Say("Дата начала спринта стендапа изменено.", chatId);
                }

                break;
        }
    }
}
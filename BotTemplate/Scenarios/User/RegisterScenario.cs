using System.Globalization;
using BotTemplate.Client;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.Telegram.Messages.Register;
using BotTemplate.Services.Telegram.Validators;
using BotTemplate.Services.YDB;
using BotTemplate.Services.YDB.Repo;
using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.Excpetions;

namespace BotTemplate.Scenarios.User;

public class RegisterScenario: IScenario
{
    private readonly ScenarioStateRepository scenarioStateRepository;
    private readonly UserAnswersRepository userAnswersRepository;
    private readonly IBackendApiClient backendApiClient;
    private readonly IMessageSender messageSender;
    private const string ScenarioId = "register";
    private const string Command = CommandsConstants.Start;

    public RegisterScenario(
        ScenarioStateRepository scenarioStateRepository,
        UserAnswersRepository userAnswersRepository,
        IBackendApiClient backendApiClient,
        IMessageSender messageSender)
    {
        this.scenarioStateRepository = scenarioStateRepository;
        this.userAnswersRepository = userAnswersRepository;
        this.backendApiClient = backendApiClient;
        this.messageSender = messageSender;
    }

    public async Task<bool> TryHandle(TelegramEvent telegramEvent,  CurrentScenarioInfo? scenarioInfo)
    {
        if (telegramEvent.Text?.Trim().ToLower() != Command && scenarioInfo?.ScenarioId != ScenarioId)
            return false;
        
        if (scenarioInfo != null && scenarioInfo.ScenarioId != ScenarioId)
        {
            await messageSender.Say("Закночи другой сценарий, прежде чем запустить стартовый сценарий", telegramEvent.ChatId);
            return true;
        }
        
        var chatId = telegramEvent.ChatId;
        var getUser = await backendApiClient.GetUser(chatId);
        if (getUser.IsSuccess)
        {
            await messageSender.Say("Ты уже зарегистрирован", chatId);
            return true;
        }
        if (getUser.HasError(ErrorStatus.NotFound))
        {
            await messageSender.Say(MessageConstants.UnknownErrorText, chatId);
            return true;
        }

        var text = telegramEvent.Text ?? "";
        var index = await scenarioStateRepository.GetIndexByChatId(chatId);
        if (index is null)
        {
            await scenarioStateRepository.StartNewScenario(chatId, ScenarioId);
            index = -1;
        }
        else
            await scenarioStateRepository.IncreaseAndGetNewMessage(chatId);

        index++;
        Message? messageToSend = null;

        switch (index.Value)
        {
            case 0:
                messageToSend = WriteYourEmailMessage.GetMessage();
                break;
            case 1:
                var emailValidator = new EmailValidator();
                if (!emailValidator.IsValid(text))
                {
                    messageToSend = new Message("Неправильный формат почты");
                    await scenarioStateRepository.DecreaseIndex(chatId);
                }
                else
                {
                    messageToSend = AreYouACoachMessage.GetMessage(text);
                    await userAnswersRepository.SaveAnswer(chatId, "Email", text);
                    await userAnswersRepository.SaveAnswer(chatId, "Username",
                        $"@{telegramEvent.Username ?? text}");
                }

                break;
            case 2:
                messageToSend = DoYouWantToCompleteSprintsMessage.GetMessage();
                if (text == "Да")
                    await userAnswersRepository.SaveAnswer(chatId, "IAmCoach", true.ToString());
                else
                    await userAnswersRepository.SaveAnswer(chatId, "IAmCoach", false.ToString());

                break;
            case 3:
                if (text is "Нет")
                {
                    messageToSend = RegisterNoSprintsMessage.GetMessage();
                    await userAnswersRepository.SaveAnswer(chatId, "SendRegularMessages", false.ToString());
                    await scenarioStateRepository.EndScenarioNoMatterWhat(chatId);
                    await RegisterUser(chatId);
                }
                else
                {
                    messageToSend = StatusTimeMessage.GetMessage();
                    await userAnswersRepository.SaveAnswer(chatId, "SendRegularMessages", true.ToString());
                }

                break;
            case 4:
                var timeRangeValidator = new TimeRangeValidator();
                if (!timeRangeValidator.IsValid(text))
                {
                    messageToSend = new Message("Неправильный формат. Нужно ввести интервал в формете 9:00-18:00");
                    await scenarioStateRepository.DecreaseIndex(chatId);
                }
                else
                {
                    messageToSend = EveningStandUpTimeMessage.GetMessage();

                    var trimmedText = text!.Trim();
                    var timeSpanStrings = trimmedText.Split('-');

                    var messageStartTime = TimeSpan.Parse(timeSpanStrings[0]).Subtract(TimeSpan.FromHours(3));
                    if (messageStartTime < TimeSpan.FromHours(0))
                        messageStartTime = messageStartTime.Add(TimeSpan.FromHours(24));

                    var messageEndTime = TimeSpan.Parse(timeSpanStrings[1]) - TimeSpan.FromHours(3);
                    if (messageEndTime < TimeSpan.FromHours(0))
                        messageEndTime = messageEndTime.Add(TimeSpan.FromHours(24));

                    await userAnswersRepository.SaveAnswer(chatId, "MessageStartTime", messageStartTime.ToString());
                    await userAnswersRepository.SaveAnswer(chatId, "MessageEndTime", messageEndTime.ToString());
                }

                break;
            case 5:
                var timeValidator = new TimeValidator();
                if (!timeValidator.IsValid(text))
                {
                    messageToSend = new Message("Неправильный формат. Нужно ввести время в формете 9:00");
                    await scenarioStateRepository.DecreaseIndex(chatId);
                }
                else
                {
                    messageToSend = CurrentStandUpDateStart.GetMessage();
                    var trimmedText = text!.Trim();

                    var eveningStandUpTime = TimeSpan.Parse(trimmedText).Subtract(TimeSpan.FromHours(3));
                    if (eveningStandUpTime < TimeSpan.FromHours(0))
                        eveningStandUpTime = eveningStandUpTime.Add(TimeSpan.FromHours(24));

                    await userAnswersRepository.SaveAnswer(chatId, "EveningStandUpTime",
                        eveningStandUpTime.ToString());
                }

                break;
            case 6:
                var dateValidator = new DateValidator();
                if (text!.Trim() == "Новый спринт")
                {
                    var coaches = await backendApiClient.GetPublicCoachListAsync();
                    messageToSend = SelectCoachMessage.GetMessage(coaches);
                    var sprintStartDate = DateTime.UtcNow.Date;
                    await userAnswersRepository.SaveAnswer(chatId, "SprintStartDate",
                        sprintStartDate.ToString(CultureInfo.InvariantCulture));

                    var firstReflectionDate = DateTime.UtcNow.Date.Add(TimeSpan.FromDays(6));
                    await userAnswersRepository.SaveAnswer(chatId, "FirstReflectionDate",
                        firstReflectionDate.ToString(CultureInfo.InvariantCulture));
                }
                else if (!dateValidator.IsValid(text))
                {
                    messageToSend = new Message("Неправильный формат. Нужно ввести дату в формете 31.01.2024");
                    await scenarioStateRepository.DecreaseIndex(chatId);
                }
                else
                {
                    var coaches = await backendApiClient.GetPublicCoachListAsync();
                    messageToSend = SelectCoachMessage.GetMessage(coaches);
                    var sprintStartDate = DateTime.ParseExact(text.Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture,
                        DateTimeStyles.None);
                    await userAnswersRepository.SaveAnswer(chatId, "SprintStartDate",
                        sprintStartDate.ToString(CultureInfo.InvariantCulture));

                    var firstReflectionDate = sprintStartDate.Add(TimeSpan.FromDays(6));
                    await userAnswersRepository.SaveAnswer(chatId, "FirstReflectionDate",
                        firstReflectionDate.ToString(CultureInfo.InvariantCulture));
                }

                break;
            case 7:
                var telegramUserIdValidator = new TelegramUserIdValidator();
                if (!telegramUserIdValidator.IsValid(text) && text != MessageConstants.WithoutCoachButtonValue)
                {
                    messageToSend = new Message("Нажми на одну из кнопок");
                    await scenarioStateRepository.DecreaseIndex(chatId);
                }
                else
                {
                    messageToSend = RegisteredMessage.GetMessage();
                    if (text != MessageConstants.WithoutCoachButtonValue)
                        await userAnswersRepository.SaveAnswer(chatId, "Coach", text!);
                    await scenarioStateRepository.EndScenarioNoMatterWhat(chatId);
                    await RegisterUser(chatId);
                }


                break;
        }

        if (messageToSend is not null)
            await messageSender.TrySay(messageToSend, chatId);

        return true;
    }

    private async Task RegisterUser(long fromChatId)
    {
        var userAnswers = await userAnswersRepository.GetAllWithKeys(fromChatId);
        var createUser = new CreateUser
        {
            UserId = fromChatId
        };
        long coachId = 0;
        foreach (var userAnswer in userAnswers)
        {
            switch (userAnswer.Key)
            {
                case "Email":
                    createUser.Email = userAnswer.Answer;
                    break;
                case "Username":
                    createUser.Username = userAnswer.Answer;
                    break;
                case "IAmCoach":
                    createUser.IAmCoach = bool.Parse(userAnswer.Answer);
                    break;
                case "SendRegularMessages":
                    createUser.SendRegularMessages = bool.Parse(userAnswer.Answer);
                    break;
                case "EveningStandUpTime":
                    createUser.EveningStandUpTime = TimeSpan.Parse(userAnswer.Answer);
                    break;
                case "MessageStartTime":
                    createUser.MessageStartTime = TimeSpan.Parse(userAnswer.Answer);
                    break;
                case "MessageEndTime":
                    createUser.MessageEndTime = TimeSpan.Parse(userAnswer.Answer);
                    break;
                case "FirstReflectionDate":
                    createUser.FirstReflectionDate = DateTime.Parse(userAnswer.Answer);
                    break;
                case "SprintStartDate":
                    createUser.SprintStartDate = DateTime.Parse(userAnswer.Answer);
                    break;
                case "Coach":
                    coachId = long.Parse(userAnswer.Answer);
                    break;
            }
        }

        await userAnswersRepository.ClearByChatId(fromChatId);
        await messageSender.Say("Создаю пользователя и таблицу. Подожди", fromChatId);
        await backendApiClient.CreateUserAsync(createUser);
        if (coachId != 0)
            await backendApiClient.GrantedAccessToUserInfoAsync(createUser.UserId, coachId);
    }
}
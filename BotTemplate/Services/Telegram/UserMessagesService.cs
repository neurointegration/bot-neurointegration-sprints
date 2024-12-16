using System.Globalization;
using BotTemplate.Client;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram.Commands;
using BotTemplate.Services.Telegram.MessageCommands;
using BotTemplate.Services.Telegram.Messages.Bottom;
using BotTemplate.Services.Telegram.Messages.Register;
using BotTemplate.Services.Telegram.Messages.Settings;
using BotTemplate.Services.Telegram.Validators;
using BotTemplate.Services.YDB;
using BotTemplate.Services.YDB.Repo;
using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotTemplate.Services.Telegram;

public class UserMessagesService
{
    private readonly IMessageView messageView;
    private readonly IBotDatabase botDatabase;
    private readonly IBackendApiClient backendApiClient;
    private readonly string telegramBotUrl;
    private readonly HttpClient client = new();

    private IChatCommandHandler[] commands;
    private IMessageCommand[] messageCommands;

    private CurrentScenarioRepository currentScenarioRepository;
    private UserAnswersRepository userAnswersRepository;
    private UsersRepository usersRepository;

    public UserMessagesService(
        IMessageView messageView,
        IBotDatabase botDatabase,
        IBackendApiClient backendApiClient,
        string telegramBotUrl)
    {
        this.messageView = messageView;
        this.botDatabase = botDatabase;
        this.backendApiClient = backendApiClient;
        this.telegramBotUrl = telegramBotUrl;
    }

    public async Task HandleMessage(Update message)
    {
        var scenariosRepository = await ScenariosRepository.Init(botDatabase);
        currentScenarioRepository = await CurrentScenarioRepository.Init(botDatabase, scenariosRepository);
        userAnswersRepository = await UserAnswersRepository.Init(botDatabase);
        usersRepository = await UsersRepository.Init(botDatabase);

        commands = new IChatCommandHandler[]
        {
            new GetAnswersCommandHandler(userAnswersRepository),
            new GetButtonsScenarioHandler(currentScenarioRepository),
            new EveningStandUpCommandHandler(currentScenarioRepository),
            new WeeklyReflectionCommandHandler(currentScenarioRepository)
        };

        messageCommands = new IMessageCommand[]
        {
            new SendStateMessage(),
            new HandleStateResponse()
        };

        var telegramEvent = message.Type switch
        {
            UpdateType.Message => new TelegramEvent
            {
                ChatId = message.Message!.Chat.Id,
                Text = message.Message.Text!,
                Username = message.Message.From!.Username!,
                MessageType = message.Message.Type
            },
            UpdateType.CallbackQuery => new TelegramEvent
            {
                ChatId = message.CallbackQuery!.Message!.Chat.Id,
                Text = message.CallbackQuery.Data!,
                Username = message.CallbackQuery.From.Username!,
                MessageType = message.CallbackQuery.Message.Type
            },
            _ => null
        };

        if (telegramEvent is null)
            await HandleDefaultUpdate(message.Message!.Chat.Id);
        else
            await HandleMessage(telegramEvent);
        
        if (message.Type is UpdateType.CallbackQuery)
            await client.GetAsync($"{telegramBotUrl}/answerCallbackQuery?callback_query_id={message.CallbackQuery!.Id}");
    }

    private async Task HandleMessage(TelegramEvent telegramEvent)
    {
        var scenarioId = await currentScenarioRepository.GetScenarioIdByChatId(telegramEvent.ChatId);

        if ((telegramEvent.Text == "/start" && scenarioId is not 0) ||
            (scenarioId is 0 && telegramEvent.Text != "/start"))
        {
            var isRegistered = await usersRepository.IsRegistered(telegramEvent.ChatId);
            if (!isRegistered)
                await HandleRegister(telegramEvent.ChatId, telegramEvent);
            else
                await SendMessage(telegramEvent.ChatId, "Ты уже зарегистрирован.");
            return;
        }

        if ((telegramEvent.Text == "Настройки" && scenarioId is not 100) ||
            (scenarioId is 100 && telegramEvent.Text != "Настройки"))
        {
            if (scenarioId != null && scenarioId != 100)
                await messageView.Say("Закночи другой сценарий, прежде чем открыть настройки.", telegramEvent.ChatId);
            else
                await HandleSettings(telegramEvent.ChatId, telegramEvent);
            return;
        }

        if (telegramEvent.Text == "Таблица результатов")
        {
            if (scenarioId != null)
                await messageView.Say("Закночи другой сценарий, прежде чем получать таблицу результатов.",
                    telegramEvent.ChatId);
            else
            {
                var sprints = await backendApiClient.GetUserSprintsAsync(telegramEvent.ChatId, telegramEvent.ChatId);
                if (sprints.Count == 0)
                {
                    await messageView.Say("У тебя пока нету таблиц.",
                        telegramEvent.ChatId);
                    return;
                }

                if (sprints.Count == 1)
                {
                    await messageView.Say($"<a href='https://docs.google.com/spreadsheets/d/{sprints.First().SheetId}'>Таблица твоих результатов и ответов</a>",
                        telegramEvent.ChatId);
                    return;
                }

                var message = sprints
                    .OrderByDescending(sprint => sprint.SprintNumber)
                    .Aggregate("Таблицы твоих результатов и ответов:\n",
                        (current, spreadSheet) => current + 
                                                  $"<a href='https://docs.google.com/spreadsheets/d/{spreadSheet.SheetId}'>Таблица твоих результатов и ответов</a>\n");
                await messageView.Say(message, telegramEvent.ChatId);
            }

            return;
        }
        
        if (telegramEvent.Text == "Мои ученики")
        {
            if (scenarioId != null)
                await messageView.Say("Закночи другой сценарий, прежде чем получать таблицы результатов твоих учеников.",
                    telegramEvent.ChatId);
            else
            {
                await messageView.Say("Загружаю твоих учеников. Подожди", telegramEvent.ChatId);
                var students = await backendApiClient.GetCoachStudentsAsync(telegramEvent.ChatId);
                if (students is null || students.Count == 0)
                {
                    await messageView.Say("У тебя пока нету учеников.",
                        telegramEvent.ChatId);
                    return;
                }
                var studentsSheets = new List<string>();
                foreach (var student in students)
                {
                    var studentSheet = (await backendApiClient.GetUserSprintsAsync(student.UserId, telegramEvent.ChatId)).MaxBy(sprint=>sprint.SprintNumber);
                    studentsSheets.Add(studentSheet.SheetId);
                }

                var message = "Таблица результатов и ответов твоих учеников:\n";
                for (var i = 0; i < students.Count; i++)
                {
                    message += $"<a href='https://docs.google.com/spreadsheets/d/{studentsSheets[i]}'>{students[i].Username}</a>\n";
                }
                
                await messageView.Say(message, telegramEvent.ChatId);
            }

            return;
        }

        if (telegramEvent.MessageType is MessageType.Text)
        {
            await HandlePlainText(telegramEvent.Text, telegramEvent.ChatId);
            return;
        }

        await HandleNonCommandMessage(telegramEvent.ChatId, telegramEvent.Text);
    }

    private async Task HandlePlainText(string text, long fromChatId)
    {
        var command = commands.FirstOrDefault(c => text.StartsWith(c.Command));
        if (command is null)
        {
            await HandleNonCommandMessage(fromChatId, text);
            return;
        }

        await HandleCommandMessage(fromChatId, command);
    }

    private async Task HandleCommandMessage(long fromChatId, IChatCommandHandler command)
    {
        var scenarioId = await currentScenarioRepository.GetScenarioIdByChatId(fromChatId);
        if (scenarioId is not null)
            await SendMessage(fromChatId, "Ответьте на вопросы, прежде чем вызывать новую команду");
        else
        {
            var message = await command.HandlePlainText(fromChatId);
            if (message != null && message.StartsWith('/'))
            {
                var messageCommand =
                    messageCommands.FirstOrDefault(messageCommand => message.StartsWith(messageCommand.Command));
                await messageCommand!.Handle(messageView, fromChatId, null);
                return;
            }

            await SendMessage(fromChatId, message);
        }
    }

    private async Task HandleNonCommandMessage(long fromChatId, string text)
    {
        var key = await currentScenarioRepository.GetCurrentKey(fromChatId);
        if (key == null)
            return;

        var currentScenarioInfo = await currentScenarioRepository.GetInfoByChatId(fromChatId);
        var scenarioType = currentScenarioInfo!.ScenarioId switch
        {
            1 => ScenarioType.Status,
            2 => ScenarioType.EveningStandUp,
            3 => ScenarioType.Reflection,
            4 => ScenarioType.Reflection,
            _ => ScenarioType.Status
        };

        var sendAnswer = new SendAnswer
        {
            UserId = fromChatId,
            Answer = text,
            AnswerNumber = currentScenarioInfo.Index!.Value,
            ScenarioType = scenarioType,
            Date = DateOnly.FromDateTime(currentScenarioInfo.Date ?? DateTime.UtcNow),
            SprintNumber = (int) currentScenarioInfo.CurrentSprintNumber!.Value,
            SprintReplyNumber = currentScenarioInfo.SprintReplyNumber!.Value
        };
        await backendApiClient.SendAnswerAsync(sendAnswer);

        var message = await currentScenarioRepository.IncreaseAndGetNewMessage(fromChatId);
        if (message != null && message.StartsWith('/'))
        {
            var messageCommand =
                messageCommands.FirstOrDefault(messageCommand => message.StartsWith(messageCommand.Command));
            var result = await messageCommand!.Handle(messageView, fromChatId, text);
            if (!result.IsSuccessful())
                await currentScenarioRepository.DecreaseIndex(fromChatId);
            else
                await currentScenarioRepository.TryEndScenario(fromChatId);
            return;
        }

        await currentScenarioRepository.TryEndScenario(fromChatId);
        await SendMessage(fromChatId, message);
    }

    private async Task HandleDefaultUpdate(long fromChatId)
    {
        await SendMessage(fromChatId, "Я пока не умею поддерживать стикеры, картинки и прочие нетекстовые сообщения!");
    }

    private async Task SendMessage(long fromChatId, string? text)
    {
        if (text is null)
            return;

        await messageView.Say(text, fromChatId);
    }

    private async Task HandleRegister(long fromChatId, TelegramEvent? telegramEvent)
    {
        var text = telegramEvent?.Text;
        var index = await currentScenarioRepository.GetIndexByChatId(fromChatId);
        if (index is null)
        {
            await currentScenarioRepository.StartNewScenarioAndGetMessage(fromChatId, 0);
            index = -1;
        }
        else
            await currentScenarioRepository.IncreaseAndGetNewMessage(fromChatId);

        index++;
        Models.Telegram.Message? messageToSend = null;

        switch (index!.Value)
        {
            case 0:
                messageToSend = WriteYourEmailMessage.GetMessage();
                break;
            case 1:
                var emailValidator = new EmailValidator();
                if (!emailValidator.IsValid(text))
                {
                    messageToSend = new Models.Telegram.Message("Неправильный формат почты.");
                    await currentScenarioRepository.DecreaseIndex(fromChatId);
                }
                else
                {
                    messageToSend = AreYouACoachMessage.GetMessage(text!);
                    await userAnswersRepository.SaveAnswer(fromChatId, "Email", text);
                    await userAnswersRepository.SaveAnswer(fromChatId, "Username", $"@{telegramEvent?.Username}" ?? text!);
                }

                break;
            case 2:
                if (text == "Да")
                {
                    messageToSend = DoYouWantToCompleteSprintsMessage.GetMessage();
                    await userAnswersRepository.SaveAnswer(fromChatId, "IAmCoach", true.ToString());
                }
                else
                {
                    await userAnswersRepository.SaveAnswer(fromChatId, "IAmCoach", false.ToString());
                    await HandleRegister(fromChatId, null);
                }

                break;
            case 3:
                if (text is "Нет")
                {
                    messageToSend = RegisterNoSprintsMessage.GetMessage();
                    await userAnswersRepository.SaveAnswer(fromChatId, "SendRegularMessages", false.ToString());
                    await currentScenarioRepository.EndScenarioNoMatterWhat(fromChatId);
                    await RegisterUser(fromChatId);
                }
                else
                {
                    messageToSend = StatusTimeMessage.GetMessage();
                    await userAnswersRepository.SaveAnswer(fromChatId, "SendRegularMessages", true.ToString());
                }

                break;
            case 4:
                var timeRangeValidator = new TimeRangeValidator();
                if (!timeRangeValidator.IsValid(text))
                {
                    messageToSend =
                        new Models.Telegram.Message("Неправильный формат. Нужно ввести интервал в формете 9:00-18:00");
                    await currentScenarioRepository.DecreaseIndex(fromChatId);
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

                    await userAnswersRepository.SaveAnswer(fromChatId, "MessageStartTime", messageStartTime.ToString());
                    await userAnswersRepository.SaveAnswer(fromChatId, "MessageEndTime", messageEndTime.ToString());
                }

                break;
            case 5:
                var timeValidator = new TimeValidator();
                if (!timeValidator.IsValid(text))
                {
                    messageToSend =
                        new Models.Telegram.Message("Неправильный формат. Нужно ввести время в формете 9:00");
                    await currentScenarioRepository.DecreaseIndex(fromChatId);
                }
                else
                {
                    messageToSend = CurrentStandUpDateStart.GetMessage();
                    var trimmedText = text!.Trim();

                    var eveningStandUpTime = TimeSpan.Parse(trimmedText).Subtract(TimeSpan.FromHours(3));
                    if (eveningStandUpTime < TimeSpan.FromHours(0))
                        eveningStandUpTime = eveningStandUpTime.Add(TimeSpan.FromHours(24));

                    await userAnswersRepository.SaveAnswer(fromChatId, "EveningStandUpTime", eveningStandUpTime.ToString());
                }

                break;
            case 6:
                var dateValidator = new DateValidator();
                if (text!.Trim() == "Новый спринт")
                {
                    var coaches = await backendApiClient.GetPublicCoachsAsync();
                    messageToSend = SelectCoachMessage.GetMessage(coaches);
                    var sprintStartDate = DateTime.UtcNow.Date;
                    await userAnswersRepository.SaveAnswer(fromChatId, "SprintStartDate",
                        sprintStartDate.ToString(CultureInfo.InvariantCulture));

                    var firstReflectionDate = DateTime.UtcNow.Date.Add(TimeSpan.FromDays(6));
                    await userAnswersRepository.SaveAnswer(fromChatId, "FirstReflectionDate",
                        firstReflectionDate.ToString(CultureInfo.InvariantCulture));
                }
                else if (!dateValidator.IsValid(text))
                {
                    messageToSend =
                        new Models.Telegram.Message("Неправильный формат. Нужно ввести дату в формете 31.01.2024");
                    await currentScenarioRepository.DecreaseIndex(fromChatId);
                }
                else
                {
                    var coaches = await backendApiClient.GetPublicCoachsAsync();
                    messageToSend = SelectCoachMessage.GetMessage(coaches);
                    var sprintStartDate = DateTime.ParseExact(text.Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture,
                        DateTimeStyles.None);
                    await userAnswersRepository.SaveAnswer(fromChatId, "SprintStartDate",
                        sprintStartDate.ToString(CultureInfo.InvariantCulture));

                    var firstReflectionDate = sprintStartDate.Add(TimeSpan.FromDays(6));
                    await userAnswersRepository.SaveAnswer(fromChatId, "FirstReflectionDate",
                        firstReflectionDate.ToString(CultureInfo.InvariantCulture));
                }

                break;
            case 7:
                var telegramUserIdValidator = new TelegramUserIdValidator();
                if (!telegramUserIdValidator.IsValid(text))
                {
                    messageToSend = new Models.Telegram.Message("Нажми на одну из кнопок");
                    await currentScenarioRepository.DecreaseIndex(fromChatId);
                }
                else
                {
                    messageToSend = RegisteredMessage.GetMessage();
                    var buttons = BottomMessage.GetMessage();
                    messageToSend.ReplyMarkup = buttons.ReplyMarkup;
                    await userAnswersRepository.SaveAnswer(fromChatId, "Coach", text!);
                    await currentScenarioRepository.EndScenarioNoMatterWhat(fromChatId);
                    await RegisterUser(fromChatId);
                }

                break;
        }

        if (messageToSend is not null)
            await messageView.SayWithMarkup(messageToSend.Text, fromChatId, messageToSend.ReplyMarkup);
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
        await messageView.Say("Создаю пользователя и таблицу. Подожди", fromChatId);
        await backendApiClient.CreateUserAsync(createUser);
        await backendApiClient.GrantedAccessToUserInfoAsync(createUser.UserId, coachId);
        await usersRepository.RegisterUser(fromChatId, createUser.IAmCoach, createUser.SendRegularMessages);
    }

    private async Task HandleSettings(long fromChatId, TelegramEvent? telegramEvent)
    {
        var text = telegramEvent?.Text;
        var index = await currentScenarioRepository.GetIndexByChatId(fromChatId);
        if (index is null)
        {
            await currentScenarioRepository.StartNewScenarioAndGetMessage(fromChatId, 100);
            index = -1;
        }
        else
            await currentScenarioRepository.IncreaseAndGetNewMessage(fromChatId);

        index++;

        var iAmCoach = await usersRepository.AmICoach(fromChatId);
        var sendRegularMessages = await usersRepository.SendRegularMessages(fromChatId);

        switch (index!.Value)
        {
            case 0:
                var message = ShowSettingsMessage.GetMessage(iAmCoach, sendRegularMessages);
                await messageView.SayWithMarkup(message.Text, fromChatId, message.ReplyMarkup);
                break;
            case 1:
                switch (text)
                {
                    case "Почта":
                        await messageView.Say(ChangeEmailMessage.GetMessage().Text, fromChatId);
                        break;
                    case "Не тренер":
                        await usersRepository.ChangeIAmCoach(fromChatId, false);
                        await currentScenarioRepository.EndScenarioNoMatterWhat(fromChatId);
                        await backendApiClient.UpdateUserAsync(new UpdateUser { UserId = fromChatId, IAmCoach = false });
                        await messageView.SayWithMarkup("Теперь ты не тренер", fromChatId, BottomMessage.GetMessage().ReplyMarkup);
                        break;
                    case "Тренер":
                        await usersRepository.ChangeIAmCoach(fromChatId, true);
                        await currentScenarioRepository.EndScenarioNoMatterWhat(fromChatId);
                        await backendApiClient.UpdateUserAsync(new UpdateUser { UserId = fromChatId, IAmCoach = true });
                        await messageView.SayWithMarkup("Теперь ты тренер", fromChatId, BottomMessage.GetMessage(true).ReplyMarkup);
                        break;
                    case "Не проходить":
                        await usersRepository.ChangeSendRegularMessages(fromChatId, false);
                        await currentScenarioRepository.EndScenarioNoMatterWhat(fromChatId);
                        await backendApiClient.UpdateUserAsync(new UpdateUser { UserId = fromChatId, SendRegularMessages = false });
                        await messageView.Say("Теперь ты больше не проходишь спринты", fromChatId);
                        break;
                    case "Проходить":
                        await usersRepository.ChangeSendRegularMessages(fromChatId, true);
                        await currentScenarioRepository.EndScenarioNoMatterWhat(fromChatId);
                        await backendApiClient.UpdateUserAsync(new UpdateUser { UserId = fromChatId, SendRegularMessages = true });
                        await messageView.Say("Теперь ты проходишь спринты. Обязательно укажи в настройках остальные данные для спринтов", fromChatId);
                        break;
                    case "Вечерний стендап":
                        await messageView.Say(ChangeEveningStandUpTimeMessage.GetMessage().Text, fromChatId);
                        for (var i = 0; i < 1; i++)
                            await currentScenarioRepository.IncreaseAndGetNewMessage(fromChatId);
                        break;
                    case "Состояние":
                        await messageView.Say(ChangeStatusTimeMessage.GetMessage().Text, fromChatId);
                        for (var i = 0; i < 2; i++)
                            await currentScenarioRepository.IncreaseAndGetNewMessage(fromChatId);
                        break;
                    case "Начало спринта":
                        await messageView.Say(ChangeCurrentStandUpDateMessage.GetMessage().Text, fromChatId);
                        for (var i = 0; i < 3; i++)
                            await currentScenarioRepository.IncreaseAndGetNewMessage(fromChatId);
                        break;
                    case "Отмена":
                        await currentScenarioRepository.EndScenarioNoMatterWhat(fromChatId);
                        break;
                }

                break;
            case 2: // изменить почту
                var emailUpdateUser = new UpdateUser
                {
                    UserId = fromChatId
                };
                var emailValidator = new EmailValidator();
                if (!emailValidator.IsValid(text))
                {
                    await messageView.Say("Неправильный формат почты.", fromChatId);
                    await currentScenarioRepository.DecreaseIndex(fromChatId);
                }
                else
                {
                    emailUpdateUser.Email = text;
                    await currentScenarioRepository.EndScenarioNoMatterWhat(fromChatId);
                    await messageView.Say("Почта изменена.", fromChatId);
                }

                break;
            case 3: // изменить время веч. стендапа
                var eveningStandUpUpdateUser = new UpdateUser
                {
                    UserId = fromChatId
                };
                var timeValidator = new TimeValidator();
                if (!timeValidator.IsValid(text))
                {
                    await messageView.Say("Неправильный формат. Нужно ввести время по МСК в формете 9:00", fromChatId);
                    await currentScenarioRepository.DecreaseIndex(fromChatId);
                }
                else
                {
                    var trimmedText = text!.Trim();

                    var eveningStandUpTime = TimeSpan.Parse(trimmedText).Subtract(TimeSpan.FromHours(3));
                    if (eveningStandUpTime < TimeSpan.FromHours(0))
                        eveningStandUpTime = eveningStandUpTime.Add(TimeSpan.FromHours(24));

                    eveningStandUpUpdateUser.EveningStandUpTime = eveningStandUpTime;
                    await currentScenarioRepository.EndScenarioNoMatterWhat(fromChatId);
                    await backendApiClient.UpdateUserAsync(eveningStandUpUpdateUser);
                    await messageView.Say("Время вечернего стендапа изменено.", fromChatId);
                }

                break;
            case 4: // изменить интервал
                var timeRangeValidator = new TimeRangeValidator();
                var timeRangeUpdateUser = new UpdateUser
                {
                    UserId = fromChatId
                };
                if (!timeRangeValidator.IsValid(text))
                {
                    await messageView.Say("Неправильный формат. Нужно ввести интервал по МСК в формете 9:00-18:00",
                        fromChatId);
                    await currentScenarioRepository.DecreaseIndex(fromChatId);
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
                    
                    await currentScenarioRepository.EndScenarioNoMatterWhat(fromChatId);
                    await backendApiClient.UpdateUserAsync(timeRangeUpdateUser);
                    await messageView.Say("Интервал опроса состояний изменен.", fromChatId);
                }

                break;
            case 5: // изменить дату начала спринта
                var sprintDateUpdateUser = new UpdateUser
                {
                    UserId = fromChatId
                };
                var dateValidator = new DateValidator();
                if (!dateValidator.IsValid(text))
                {
                    await messageView.Say("Неправильный формат. Нужно ввести дату в формете 06.12.2024", fromChatId);
                    await currentScenarioRepository.DecreaseIndex(fromChatId);
                }
                else
                {
                    var sprintStartDate = DateTime.ParseExact(text.Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture,
                        DateTimeStyles.None);
                    sprintDateUpdateUser.SprintStartDate = sprintStartDate;

                    var firstReflectionDate = sprintStartDate.Add(TimeSpan.FromDays(6));
                    sprintDateUpdateUser.ReflectionDate = firstReflectionDate;
                    
                    await currentScenarioRepository.EndScenarioNoMatterWhat(fromChatId);
                    await messageView.Say("Дата начала спринта стендапа изменено.", fromChatId);
                }

                break;
        }
    }
}
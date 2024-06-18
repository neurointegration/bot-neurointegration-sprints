using System.Globalization;
using BotTemplate.Client;
using BotTemplate.Models.ClientDto;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram.Commands;
using BotTemplate.Services.Telegram.MessageCommands;
using BotTemplate.Services.Telegram.Messages.Bottom;
using BotTemplate.Services.Telegram.Messages.Register;
using BotTemplate.Services.Telegram.Messages.Settings;
using BotTemplate.Services.Telegram.Validators;
using BotTemplate.Services.YDB;
using BotTemplate.Services.YDB.Repo;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotTemplate.Services.Telegram;

public class HandleUpdateService
{
    private readonly IMessageView _messageView;
    private readonly IBotDatabase _botDatabase;
    private readonly IBackendApiClient _backendApiClient;
    private readonly string _telegramBotUrl;
    private readonly HttpClient _client = new();
    
    private IChatCommandHandler[] _commands;
    private IMessageCommand[] _messageCommands;
    
    private CurrentScenarioRepo _currentScenarioRepo;
    private UserAnswersRepo _userAnswersRepo;
    private UsersRepo _usersRepo;

    public HandleUpdateService(
        IMessageView messageView,
        IBotDatabase botDatabase,
        IBackendApiClient backendApiClient,
        string telegramBotUrl)
    {
        _messageView = messageView;
        _botDatabase = botDatabase;
        _backendApiClient = backendApiClient;
        _telegramBotUrl = telegramBotUrl;
    }

    public async Task Handle(Update update)
    {
        var scenariosRepo = await ScenariosRepo.InitWithDatabase(_botDatabase);
        _currentScenarioRepo = await CurrentScenarioRepo.InitWithDatabase(_botDatabase, scenariosRepo);
        _userAnswersRepo = await UserAnswersRepo.InitWithDatabase(_botDatabase);
        _usersRepo = await UsersRepo.InitWithDatabase(_botDatabase);
        
        _commands = new IChatCommandHandler[]
        {
            new GetAnswersCommandHandler(_userAnswersRepo),
            new GetButtonsScenarioHandler(_currentScenarioRepo),
            new EveningStandUpCommandHandler(_currentScenarioRepo),
            new WeeklyReflectionCommandHandler(_currentScenarioRepo)
        };
        
        _messageCommands = new IMessageCommand[]
        {
            new SendStateMessage(),
            new HandleStateResponse()
        };

        var telegramEvent = update.Type switch
        {
            UpdateType.Message => new TelegramEvent
            {
                ChatId = update.Message!.Chat.Id,
                Text = update.Message.Text!,
                Username = update.Message.From!.Username!,
                MessageType = update.Message.Type
            },
                UpdateType.CallbackQuery => new TelegramEvent
            {
                ChatId = update.CallbackQuery!.Message!.Chat.Id,
                Text = update.CallbackQuery.Data!,
                Username = update.CallbackQuery.From.Username!,
                MessageType = update.CallbackQuery.Message.Type
            },
            _ => null
        };
        

        if (update.Type is UpdateType.CallbackQuery)
        {
            await _client.GetAsync($"{_telegramBotUrl}/answerCallbackQuery?callback_query_id={update.CallbackQuery!.Id}");
        }

        if (telegramEvent is null)
        {
            await HandleDefaultUpdate(update.Message!.Chat.Id);
            return;
        }

        await HandleMessage(telegramEvent);
    }

    private async Task HandleMessage(TelegramEvent telegramEvent)
    {
        var scenarioId = await _currentScenarioRepo.GetScenarioIdByChatId(telegramEvent.ChatId);
        
        if (telegramEvent.Text == "/start" || scenarioId is 0)
        {
            var isRegistered = await _usersRepo.IsRegistered(telegramEvent.ChatId);
            if (!isRegistered)
                await HandleRegister(telegramEvent.ChatId, telegramEvent);
            else
                await SendMessage(telegramEvent.ChatId, "Ты уже зарегистрирован.");
            return;
        }

        if (telegramEvent.Text == "Настройки" || scenarioId is 100)
        {
            if (scenarioId != null && scenarioId != 100)
                await _messageView.Say("Закночи другой сценарий, прежде чем открыть настройки.", telegramEvent.ChatId);
            else
                await HandleSettings(telegramEvent.ChatId, telegramEvent);
            return;
        }
        
        if (telegramEvent.Text == "Таблица результатов")
        {
            if (scenarioId != null)
                await _messageView.Say("Закночи другой сценарий, прежде чем получать таблицу результатов.",
                    telegramEvent.ChatId);
            else
            {
                // var spreadSheets = await _backendApiClient.GetUserSpreadSheetsAsync(telegramEvent.ChatId, telegramEvent.ChatId);
                var spreadSheets = new List<string> { "www.link1.com" };
                if (spreadSheets is null || spreadSheets.Count == 0)
                {
                    await _messageView.Say("У тебя пока нету таблиц.",
                        telegramEvent.ChatId);
                    return;
                }

                if (spreadSheets.Count == 1)
                {
                    await _messageView.Say($"Таблица твоих результатов и ответов:\n{spreadSheets}",
                        telegramEvent.ChatId);
                    return;
                }

                var message = spreadSheets.Aggregate("Таблицы твоих результатов и ответов:\n", (current, spreadSheet) => current + (spreadSheet + '\n'));
                await _messageView.Say(message,
                    telegramEvent.ChatId);
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
        var command = _commands.FirstOrDefault(c => text.StartsWith(c.Command));
        if (command is null)
        {
            await HandleNonCommandMessage(fromChatId, text);
            return;
        }

        await HandleCommandMessage(fromChatId, command);
    }
    
    private async Task HandleCommandMessage(long fromChatId, IChatCommandHandler command)
    {
        var scenarioId = await _currentScenarioRepo.GetScenarioIdByChatId(fromChatId);
        if (scenarioId is not null)
            await SendMessage(fromChatId, "Ответьте на вопросы, прежде чем вызывать новую команду");
        else
        {
            var message = await command.HandlePlainText(fromChatId);
            if (message != null && message.StartsWith('/'))
            {
                var messageCommand = _messageCommands.FirstOrDefault(messageCommand => message.StartsWith(messageCommand.Command));
                await messageCommand!.Handle(_messageView, fromChatId, null);
                return;
            }
            await SendMessage(fromChatId, message);
            
        }
    }

    private async Task HandleNonCommandMessage(long fromChatId, string text)
    {
        var key = await _currentScenarioRepo.GetCurrentKey(fromChatId);
        if (key == null) 
            return;
        
        await _userAnswersRepo.SaveAnswer(fromChatId, key, text);
        var message = await _currentScenarioRepo.IncreaseAndGetNewMessage(fromChatId);
        if (message != null && message.StartsWith('/'))
        {
            var messageCommand = _messageCommands.FirstOrDefault(messageCommand => message.StartsWith(messageCommand.Command));
            var result = await messageCommand!.Handle(_messageView, fromChatId, text);
            if (!result.IsSuccessful())
                await _currentScenarioRepo.DecreaseIndex(fromChatId);
            else
                await _currentScenarioRepo.TryEndScenario(fromChatId);
            return;
        }
        
        await _currentScenarioRepo.TryEndScenario(fromChatId);
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
        
        await _messageView.Say(text, fromChatId);
    }

    private async Task HandleRegister(long fromChatId, TelegramEvent? telegramEvent)
    {
        var text = telegramEvent?.Text;
        var index = await _currentScenarioRepo.GetIndexByChatId(fromChatId);
        if (index is null)
        {
            await _currentScenarioRepo.StartNewScenarioAndGetMessage(fromChatId, 0);
            index = -1;
        }
        else
            await _currentScenarioRepo.IncreaseAndGetNewMessage(fromChatId);

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
                    await _currentScenarioRepo.DecreaseIndex(fromChatId);
                }
                else
                {
                    messageToSend = AreYouACoachMessage.GetMessage(text!);
                    await _userAnswersRepo.SaveAnswer(fromChatId, "Email", text);
                    await _userAnswersRepo.SaveAnswer(fromChatId, "Username", $"@{telegramEvent?.Username}" ?? text!);
                }
                
                break;
            case 2:
                if (text == "Да")
                {
                    messageToSend = DoYouWantToCompleteSprintsMessage.GetMessage();
                    await _userAnswersRepo.SaveAnswer(fromChatId, "IAmCoach", true.ToString());
                }
                else
                {
                    await _userAnswersRepo.SaveAnswer(fromChatId, "IAmCoach", false.ToString());
                    await HandleRegister(fromChatId, null);
                }
                break;
            case 3:
                if (text is "Нет")
                {
                    messageToSend = RegisterNoSprintsMessage.GetMessage();
                    await _userAnswersRepo.SaveAnswer(fromChatId, "SendRegularMessages", false.ToString());
                    await _currentScenarioRepo.EndScenarioNoMatterWhat(fromChatId);
                    await RegisterUser(fromChatId);
                }
                else
                {
                    messageToSend = StatusTimeMessage.GetMessage();
                    await _userAnswersRepo.SaveAnswer(fromChatId, "SendRegularMessages", true.ToString());
                }
                
                break;
            case 4:
                var timeRangeValidator = new TimeRangeValidator();
                if (!timeRangeValidator.IsValid(text))
                {
                    messageToSend = new Models.Telegram.Message("Неправильный формат. Нужно ввести интервал в формете 9:00-18:00");
                    await _currentScenarioRepo.DecreaseIndex(fromChatId);
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
                    
                    await _userAnswersRepo.SaveAnswer(fromChatId, "MessageStartTime", messageStartTime.ToString());
                    await _userAnswersRepo.SaveAnswer(fromChatId, "MessageEndTime", messageEndTime.ToString());
                }
                
                break;
            case 5:
                var timeValidator = new TimeValidator();
                if (!timeValidator.IsValid(text))
                {
                    messageToSend = new Models.Telegram.Message("Неправильный формат. Нужно ввести время в формете 9:00");
                    await _currentScenarioRepo.DecreaseIndex(fromChatId);
                }
                else
                {
                    messageToSend = CurrentStandUpDateStart.GetMessage();
                    var trimmedText = text!.Trim();
                    
                    var eveningStandUpTime = TimeSpan.Parse(trimmedText).Subtract(TimeSpan.FromHours(3));
                    if (eveningStandUpTime < TimeSpan.FromHours(0))
                        eveningStandUpTime = eveningStandUpTime.Add(TimeSpan.FromHours(24));

                    await _userAnswersRepo.SaveAnswer(fromChatId, "EveningStandUpTime", eveningStandUpTime.ToString());
                }
                
                break;
            case 6:
                var dateValidator = new DateValidator();
                if (text!.Trim() == "Новый спринт")
                {
                    messageToSend = SelectCoachMessage.GetMessage(new List<ApiUser> { new() { UserId = 228, Username = "Test" } });
                    var sprintStartDate = DateTime.UtcNow.Date;
                    await _userAnswersRepo.SaveAnswer(fromChatId, "SprintStartDate", sprintStartDate.ToString(CultureInfo.InvariantCulture));
                    
                    var firstReflectionDate = DateTime.UtcNow.Date.Add(TimeSpan.FromDays(6));
                    await _userAnswersRepo.SaveAnswer(fromChatId, "FirstReflectionDate", firstReflectionDate.ToString(CultureInfo.InvariantCulture));
                }
                else if (!dateValidator.IsValid(text))
                {
                    messageToSend = new Models.Telegram.Message("Неправильный формат. Нужно ввести дату в формете 31.01.2024");
                    await _currentScenarioRepo.DecreaseIndex(fromChatId);
                }
                else
                {
                    // var coaches = await _backendApiClient.GetPublicCoachsAsync();
                    var coaches = new List<ApiUser> { new() { UserId = 228, Username = "Test" } };
                    messageToSend = SelectCoachMessage.GetMessage(coaches);
                    var sprintStartDate = DateTime.ParseExact(text.Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
                    await _userAnswersRepo.SaveAnswer(fromChatId, "SprintStartDate", sprintStartDate.ToString(CultureInfo.InvariantCulture));
                    
                    var firstReflectionDate = sprintStartDate.Add(TimeSpan.FromDays(6));
                    await _userAnswersRepo.SaveAnswer(fromChatId, "FirstReflectionDate", firstReflectionDate.ToString(CultureInfo.InvariantCulture));
                }
                
                break;
            case 7:
                var telegramUserIdValidator = new TelegramUserIdValidator();
                if (!telegramUserIdValidator.IsValid(text))
                {
                    messageToSend = new Models.Telegram.Message("Нажми на одну из кнопок");
                    await _currentScenarioRepo.DecreaseIndex(fromChatId);
                }
                else
                {
                    messageToSend = RegisteredMessage.GetMessage();
                    var buttons = BottomMessage.GetMessage();
                    messageToSend.ReplyMarkup = buttons.ReplyMarkup;
                    await _userAnswersRepo.SaveAnswer(fromChatId, "Coach", text!);
                    await _currentScenarioRepo.EndScenarioNoMatterWhat(fromChatId);
                    await RegisterUser(fromChatId);
                }
                
                break;
        }

        if (messageToSend is not null)
            await _messageView.SayWithMarkup(messageToSend.Text, fromChatId, messageToSend.ReplyMarkup);
    }

    private async Task RegisterUser(long fromChatId)
    {
        var userAnswers = await _userAnswersRepo.GetAllWithKeys(fromChatId);
        var createUser = new CreateUser
        {
            UserId = fromChatId
        };
        long coachId;
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
        
        await _usersRepo.RegisterUser(fromChatId, createUser.IAmCoach, createUser.SendRegularMessages);
        // await _backendApiClient.CreateUserAsync(createUser);
        // await _backendApiClient.GrantedAccessToUserInfoAsync(createUser.UserId, coachId);
        await _userAnswersRepo.ClearByChatId(fromChatId);
    }
    
    private async Task HandleSettings(long fromChatId, TelegramEvent? telegramEvent)
    {
        var text = telegramEvent?.Text;
        var index = await _currentScenarioRepo.GetIndexByChatId(fromChatId);
        if (index is null)
        {
            await _currentScenarioRepo.StartNewScenarioAndGetMessage(fromChatId, 100);
            index = -1;
        }
        else
            await _currentScenarioRepo.IncreaseAndGetNewMessage(fromChatId);

        index++;

        var iAmCoach = await _usersRepo.AmICoach(fromChatId);
        var sendRegularMessages = await _usersRepo.SendRegularMessages(fromChatId);
        
        switch (index!.Value)
        {
            case 0:
                var message = ShowSettingsMessage.GetMessage(iAmCoach, sendRegularMessages);
                await _messageView.SayWithMarkup(message.Text, fromChatId, message.ReplyMarkup);
                break;
            case 1:
                switch (text)
                {
                    case "Почта":
                        await _messageView.Say(ChangeEmailMessage.GetMessage().Text, fromChatId);
                        break;
                    case "Не тренер":
                        await _usersRepo.ChangeIAmCoach(fromChatId, false);
                        // await _backendApiClient.UpdateUserAsync(new UpdateUser { UserId = fromChatId, IAmCoach = false });
                        await _currentScenarioRepo.EndScenarioNoMatterWhat(fromChatId);
                        await _messageView.SayWithMarkup("Теперь ты не тренер", fromChatId, BottomMessage.GetMessage().ReplyMarkup);
                        break;
                    case "Тренер":
                        await _usersRepo.ChangeIAmCoach(fromChatId, true);
                        // await _backendApiClient.UpdateUserAsync(new UpdateUser { UserId = fromChatId, IAmCoach = true });
                        await _currentScenarioRepo.EndScenarioNoMatterWhat(fromChatId);
                        await _messageView.SayWithMarkup("Теперь ты тренер", fromChatId, BottomMessage.GetMessage(true).ReplyMarkup);
                        break;
                    case "Не проходить":
                        await _usersRepo.ChangeSendRegularMessages(fromChatId, false);
                        // await _backendApiClient.UpdateUserAsync(new UpdateUser { UserId = fromChatId, SendRegularMessages = false });
                        await _currentScenarioRepo.EndScenarioNoMatterWhat(fromChatId);
                        await _messageView.Say("Теперь ты больше не проходишь спринты", fromChatId);
                        break;
                    case "Проходить":
                        await _usersRepo.ChangeSendRegularMessages(fromChatId, true);
                        // await _backendApiClient.UpdateUserAsync(new UpdateUser { UserId = fromChatId, SendRegularMessages = true });
                        await _currentScenarioRepo.EndScenarioNoMatterWhat(fromChatId);
                        await _messageView.Say("Теперь ты проходишь спринты. Обязательно укажи в настройках остальные данные для спринтов", fromChatId);
                        break;
                    case "Вечерний стендап":
                        await _messageView.Say(ChangeEveningStandUpTimeMessage.GetMessage().Text, fromChatId);
                        for (var i = 0; i < 1; i++)
                            await _currentScenarioRepo.IncreaseAndGetNewMessage(fromChatId);
                        break;
                    case "Состояние":
                        await _messageView.Say(ChangeStatusTimeMessage.GetMessage().Text, fromChatId);
                        for (var i = 0; i < 2; i++)
                            await _currentScenarioRepo.IncreaseAndGetNewMessage(fromChatId);
                        break;
                    case "Начало спринта":
                        await _messageView.Say(ChangeCurrentStandUpDateMessage.GetMessage().Text, fromChatId);
                        for (var i = 0; i < 3; i++)
                            await _currentScenarioRepo.IncreaseAndGetNewMessage(fromChatId);
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
                    await _messageView.Say("Неправильный формат почты.", fromChatId);
                    await _currentScenarioRepo.DecreaseIndex(fromChatId);
                }
                else
                {
                    emailUpdateUser.Email = text;
                    // await _backendApiClient.UpdateUserAsync(emailUpdateUser);
                    await _currentScenarioRepo.EndScenarioNoMatterWhat(fromChatId);
                    await _messageView.Say("Почта изменена.", fromChatId);
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
                    await _messageView.Say("Неправильный формат. Нужно ввести время в формете 9:00", fromChatId);
                    await _currentScenarioRepo.DecreaseIndex(fromChatId);
                }
                else
                {
                    var trimmedText = text!.Trim();
                    
                    var eveningStandUpTime = TimeSpan.Parse(trimmedText).Subtract(TimeSpan.FromHours(3));
                    if (eveningStandUpTime < TimeSpan.FromHours(0))
                        eveningStandUpTime = eveningStandUpTime.Add(TimeSpan.FromHours(24));

                    eveningStandUpUpdateUser.EveningStandUpTime = eveningStandUpTime;
                    // await _backendApiClient.UpdateUserAsync(eveningStandUpUpdateUser);
                    await _currentScenarioRepo.EndScenarioNoMatterWhat(fromChatId);
                    await _messageView.Say("Время вечернего стендапа изменено.", fromChatId);
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
                    await _messageView.Say("Неправильный формат. Нужно ввести интервал в формете 9:00-18:00", fromChatId);
                    await _currentScenarioRepo.DecreaseIndex(fromChatId);
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
                    
                    // await _backendApiClient.UpdateUserAsync(timeRangeUpdateUser);
                    await _currentScenarioRepo.EndScenarioNoMatterWhat(fromChatId);
                    await _messageView.Say("Интервал опроса состояний изменен.", fromChatId);
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
                    await _messageView.Say("Неправильный формат. Нужно ввести дату в формете 31.01.2024", fromChatId);
                    await _currentScenarioRepo.DecreaseIndex(fromChatId);
                }
                else
                {
                    var sprintStartDate = DateTime.ParseExact(text.Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
                    sprintDateUpdateUser.SprintStartDate = sprintStartDate;
                    
                    var firstReflectionDate = sprintStartDate.Add(TimeSpan.FromDays(6));
                    sprintDateUpdateUser.ReflectionDate = firstReflectionDate;
                    
                    // await _backendApiClient.UpdateUserAsync(sprintDateUpdateUser);
                    await _currentScenarioRepo.EndScenarioNoMatterWhat(fromChatId);
                    await _messageView.Say("Дата начала спринта стендапа изменено.", fromChatId);
                }
                break;
        }
    }
}
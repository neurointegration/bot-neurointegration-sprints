using System.Globalization;
using BotTemplate.Models.ClientDto;
using BotTemplate.Services.S3Storage;
using BotTemplate.Services.Telegram.Commands;
using BotTemplate.Services.Telegram.MessageCommands;
using BotTemplate.Services.Telegram.Messages;
using BotTemplate.Services.Telegram.Messages.Register;
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
    
    private IChatCommandHandler[] _commands;
    private IMessageCommand[] _messageCommands;
    
    private CurrentScenarioRepo _currentScenarioRepo;
    private UserAnswersRepo _userAnswersRepo;

    public HandleUpdateService(
        IMessageView messageView,
        IBotDatabase botDatabase)
    {
        _messageView = messageView;
        _botDatabase = botDatabase;
    }

    public async Task Handle(Update update)
    {
        var scenariosRepo = await ScenariosRepo.InitWithDatabase(_botDatabase);
        _currentScenarioRepo = await CurrentScenarioRepo.InitWithDatabase(_botDatabase, scenariosRepo);
        _userAnswersRepo = await UserAnswersRepo.InitWithDatabase(_botDatabase);
        
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
        
        var handler = update.Type switch
        {
            UpdateType.Message => HandleMessage(update.Message!),
            UpdateType.CallbackQuery => HandlePlainText(update.CallbackQuery!.Data!, update.CallbackQuery!.Message!.Chat.Id),
            _ => HandleDefaultUpdate(update.Message!.Chat.Id)
        };

        await handler;
    }

    private async Task HandleMessage(Message message)
    {
        var scenarioId = await _currentScenarioRepo.GetScenarioIdByChatId(message.Chat.Id);
        
        if (message.Text! == "/start" || scenarioId is 0)
        {
            await HandleRegister(message.Chat.Id, message);
            return;
        }
        
        if (message.Type == MessageType.Text)
        {
            await HandlePlainText(message.Text!, message.Chat.Id);
            return;
        }

        await HandleNonCommandMessage(message.Chat.Id, message.Text!);
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

    private async Task HandleRegister(long fromChatId, Message message)
    {
        var text = message.Text;
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
                messageToSend = AreYouACoachMessage.GetMessage();
                await _userAnswersRepo.SaveAnswer(fromChatId, "Email", text!);
                await _userAnswersRepo.SaveAnswer(fromChatId, "Username", message.From?.Username ?? text!);
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
                    var firstReflectionDate = DateTime.UtcNow.Date.Add(TimeSpan.FromDays(6));

                    await _userAnswersRepo.SaveAnswer(fromChatId, "SprintStartDate", sprintStartDate.ToString(CultureInfo.InvariantCulture));
                    await _userAnswersRepo.SaveAnswer(fromChatId, "FirstReflectionDate", firstReflectionDate.ToString(CultureInfo.InvariantCulture));
                }
                else if (!dateValidator.IsValid(text))
                {
                    messageToSend = new Models.Telegram.Message("Неправильный формат. Нужно ввести дату в формете 31.01.2024");
                    await _currentScenarioRepo.DecreaseIndex(fromChatId);
                }
                else
                {
                    messageToSend = SelectCoachMessage.GetMessage(new List<ApiUser> { new() { UserId = 228, Username = "Test" } } );
                    var sprintStartDate = DateTime.Parse(text.Trim());
                    var firstReflectionDate = sprintStartDate.Add(TimeSpan.FromDays(6));

                    await _userAnswersRepo.SaveAnswer(fromChatId, "SprintStartDate", sprintStartDate.ToString(CultureInfo.InvariantCulture));
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
                    await _userAnswersRepo.SaveAnswer(fromChatId, "Coach", text!);
                    await _currentScenarioRepo.EndScenarioNoMatterWhat(fromChatId);
                }
                
                break;
        }

        if (messageToSend is not null)
            await _messageView.SayWithMarkup(messageToSend.Text, fromChatId, messageToSend.ReplyMarkup);
    }

    private async Task RegisterUser(long fromChatId)
    {
    }
}
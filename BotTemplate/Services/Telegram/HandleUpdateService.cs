using BotTemplate.Services.S3Storage;
using BotTemplate.Services.Telegram.Commands;
using BotTemplate.Services.Telegram.MessageCommands;
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
            new StartCommandHandler(_currentScenarioRepo),
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
}
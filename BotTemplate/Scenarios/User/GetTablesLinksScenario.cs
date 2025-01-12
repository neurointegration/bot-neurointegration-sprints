using BotTemplate.Client;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;

namespace BotTemplate.Scenarios.User;

public class GetTablesLinksScenario: IScenario
{
    private readonly IBackendApiClient backendApiClient;
    private readonly IMessageSender messageSender;

    private const string Command = CommandsConstants.ResultTables;

    public GetTablesLinksScenario(IBackendApiClient backendApiClient, IMessageSender messageSender)
    {
        this.backendApiClient = backendApiClient;
        this.messageSender = messageSender;
    }

    public async Task<bool> TryHandle(TelegramEvent telegramEvent, CurrentScenarioInfo? scenarioInfo)
    {
        if (telegramEvent.Text?.Trim().ToLower() != Command)
            return false;
        
        if (scenarioInfo != null)
        {
            await messageSender.Say("Закночи другой сценарий, прежде чем получать таблицу результатов.", telegramEvent.ChatId);
            return true;
        }
        
        var sprints = await backendApiClient.GetUserSprintsAsync(telegramEvent.ChatId, telegramEvent.ChatId);
        if (sprints.Count == 0)
        {
            await messageSender.Say("У тебя пока нету таблиц.",
                telegramEvent.ChatId);
            return true;
        }

        if (sprints.Count == 1)
        {
            await messageSender.Say(
                $"<a href='https://docs.google.com/spreadsheets/d/{sprints.First().SheetId}'>Таблица твоих результатов и ответов</a>",
                telegramEvent.ChatId);
            return true;
        }

        var message = sprints
            .OrderByDescending(sprint => sprint.SprintNumber)
            .Aggregate("Таблицы твоих результатов и ответов:\n",
                (current, spreadSheet) => current +
                                          $"<a href='https://docs.google.com/spreadsheets/d/{spreadSheet.SheetId}'>Таблица твоих результатов и ответов</a>\n");
        await messageSender.Say(message, telegramEvent.ChatId);

        return true;
    }
}
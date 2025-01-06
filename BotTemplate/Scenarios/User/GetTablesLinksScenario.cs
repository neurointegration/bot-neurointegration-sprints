using BotTemplate.Client;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;

namespace BotTemplate.Scenarios.User;

public class GetTablesLinksScenario
{
    private readonly IBackendApiClient backendApiClient;
    private readonly IMessageSender messageSender;

    public GetTablesLinksScenario(IBackendApiClient backendApiClient, IMessageSender messageSender)
    {
        this.backendApiClient = backendApiClient;
        this.messageSender = messageSender;
    }

    public async Task Handle(TelegramEvent telegramEvent)
    {
        var sprints = await backendApiClient.GetUserSprintsAsync(telegramEvent.ChatId, telegramEvent.ChatId);
        if (sprints.Count == 0)
        {
            await messageSender.Say("У тебя пока нету таблиц.",
                telegramEvent.ChatId);
            return;
        }

        if (sprints.Count == 1)
        {
            await messageSender.Say(
                $"<a href='https://docs.google.com/spreadsheets/d/{sprints.First().SheetId}'>Таблица твоих результатов и ответов</a>",
                telegramEvent.ChatId);
            return;
        }

        var message = sprints
            .OrderByDescending(sprint => sprint.SprintNumber)
            .Aggregate("Таблицы твоих результатов и ответов:\n",
                (current, spreadSheet) => current +
                                          $"<a href='https://docs.google.com/spreadsheets/d/{spreadSheet.SheetId}'>Таблица твоих результатов и ответов</a>\n");
        await messageSender.Say(message, telegramEvent.ChatId);
    }
}
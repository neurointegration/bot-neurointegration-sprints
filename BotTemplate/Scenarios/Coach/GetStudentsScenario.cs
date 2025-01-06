using BotTemplate.Client;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;

namespace BotTemplate.Scenarios.Coach;

public class GetStudentsScenario
{
    private readonly IBackendApiClient backendApiClient;
    private readonly IMessageSender messageSender;

    public GetStudentsScenario(IBackendApiClient backendApiClient, IMessageSender messageSender)
    {
        this.backendApiClient = backendApiClient;
        this.messageSender = messageSender;
    }

    public async Task Handle(TelegramEvent telegramEvent)
    {
        await messageSender.Say("Загружаю твоих учеников. Подожди", telegramEvent.ChatId);
        var students = await backendApiClient.GetCoachStudentsAsync(telegramEvent.ChatId);
        if (students.Count == 0)
        {
            await messageSender.Say("У тебя пока нету учеников.",
                telegramEvent.ChatId);
            return;
        }

        var studentsSheets = new List<string>();
        foreach (var student in students)
        {
            var studentSheet =
                (await backendApiClient.GetUserSprintsAsync(student.UserId, telegramEvent.ChatId)).MaxBy(
                    sprint => sprint.SprintNumber);
            studentsSheets.Add(studentSheet.SheetId);
        }

        var message = "Таблица результатов и ответов твоих учеников:\n";
        for (var i = 0; i < students.Count; i++)
        {
            message +=
                $"<a href='https://docs.google.com/spreadsheets/d/{studentsSheets[i]}'>{students[i].Username}</a>\n";
        }

        await messageSender.Say(message, telegramEvent.ChatId);
    }
}
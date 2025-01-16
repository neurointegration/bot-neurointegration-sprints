using Neurointegration.Api.DataModels.Models;

namespace BotTemplate.Models.ScenariosData;

public class ScenarioToStart
{
    public long ChatId {get; set; }
    public string ScenarioId { get; set; }
    public ScenarioType ScenarioType { get; set; }
    public DateTime? Date { get; set; }
    public long SprintNumber { get; set; }
    public int SprintReplyNumber { get; set; }
    public string? Data { get; set; }

    public ScenarioToStart(
        long chatId,
        string scenarioId,
        ScenarioType scenarioType,
        DateTime? date,
        long sprintNumber,
        int sprintReplyNumber,
        string? data
    )
    {
        ChatId = chatId;
        ScenarioId = scenarioId;
        ScenarioType = scenarioType;
        Date = date;
        SprintNumber = sprintNumber;
        SprintReplyNumber = sprintReplyNumber;
        Data = data;
    }
}
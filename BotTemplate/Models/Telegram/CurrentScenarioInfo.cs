namespace BotTemplate.Models.Telegram;

public class CurrentScenarioInfo
{
    public long ChatId { get; set; }
    public long ScenarioId { get; set; }
    public long? CurrentSprintNumber { get; set; }
    public int? SprintReplyNumber { get; set; }
    public int? Index { get; set; }
    public DateTime? Date { get; set; }
}
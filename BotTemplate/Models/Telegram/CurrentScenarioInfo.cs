namespace BotTemplate.Models.Telegram;

public class CurrentScenarioInfo
{
    public long ChatId { get; set; }
    public string ScenarioId { get; set; } = default!;
    public long? CurrentSprintNumber { get; set; }
    public int? SprintReplyNumber { get; set; }
    public int? Index { get; set; }
    public DateTime? Date { get; set; }
}
namespace BotTemplate.Models.ClientDto;

public class Question
{
    public DateTime Date { get; set; }
    public long UserId { get; set; }
    public ScenarioType ScenarioType { get; set; }
    public long SprintNumber { get; set; }
    public int SprintReplyNumber { get; set; }
    public int Priority { get; set; }
}
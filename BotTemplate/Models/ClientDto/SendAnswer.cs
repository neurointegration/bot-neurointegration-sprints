namespace BotTemplate.Models.ClientDto;

public class SendAnswer
{
    public long UserId { get; set; }
    public DateOnly Date { get; set; }
    public string Answer { get; set; }
    public ScenarioType ScenarioType { get; set; }
    public bool ReplaceValue { get; set; } = false;
    public int AnswerNumber { get; set; }
}
using Neurointegration.Api.DataModels.Models;

namespace Neurointegration.Api.DataModels.Dto;

public record SendAnswer
{
    public long UserId { get; set; }
    public DateOnly Date { get; set; }
    public string Answer { get; set; }
    public ScenarioType ScenarioType { get; set; }
    public int AnswerNumber { get; set; }
    public int SprintNumber { get; set; }
    public int SprintReplyNumber { get; set; }
}
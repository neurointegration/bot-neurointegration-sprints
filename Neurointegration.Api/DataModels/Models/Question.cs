namespace Neurointegration.Api.DataModels.Models;

public class Question
{
    private Question()
    {
    }

    public Question(
        DateTime date,
        long userId,
        ScenarioType scenarioType,
        long sprintNumber,
        int sprintReplyNumber,
        int priority
    )
    {
        Date = date;
        UserId = userId;
        ScenarioType = scenarioType;
        SprintNumber = sprintNumber;
        SprintReplyNumber = sprintReplyNumber;
        Priority = priority;
    }

    public Question(Question question)
    {
        Date = question.Date;
        UserId = question.UserId;
        ScenarioType = question.ScenarioType;
        SprintNumber = question.SprintNumber;
        SprintReplyNumber = question.SprintReplyNumber;
        Priority = question.Priority;
    }

    public DateTime Date { get; set; }
    public long UserId { get; set; }
    public ScenarioType ScenarioType { get; set; }
    public long SprintNumber { get; set; }
    public int SprintReplyNumber { get; set; }
    public int Priority { get; set; }
}
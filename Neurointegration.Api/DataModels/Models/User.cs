using Neurointegration.Api.DataModels.Dto;

namespace Neurointegration.Api.DataModels.Models;

public class User
{
    public User()
    {
    }


    public User(CreateUser createUser)
    {
        UserId = createUser.UserId;
        Email = createUser.Email;
        Username = createUser.Username;
        IAmCoach = createUser.IAmCoach;
        SendRegularMessages = createUser.SendRegularMessages;
        EveningStandUpTime = createUser.EveningStandUpTime;
        WeekReflectionTime = createUser.EveningStandUpTime;
        MessageStartTime = createUser.MessageStartTime;
        MessageEndTime = createUser.MessageEndTime;
    }

    public long UserId { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public bool IAmCoach { get; set; }
    public bool SendRegularMessages { get; set; }
    public TimeSpan? EveningStandUpTime { get; set; }
    public TimeSpan? WeekReflectionTime { get; set; }
    public TimeSpan? MessageStartTime { get; set; }
    public TimeSpan? MessageEndTime { get; set; }
    public List<Sprint> Sprints { get; set; } = new List<Sprint>();
    public List<RoutineAction> RoutineActions { get; set; } = new List<RoutineAction>();
}
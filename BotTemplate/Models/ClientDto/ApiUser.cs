namespace BotTemplate.Models.ClientDto;

public class ApiUser
{
    public ApiUser()
    {
    }

    public long UserId { get; set; }

    public string Email { get; set; }
    public bool IAmCoach { get; set; }
    public bool SendRegularMessages { get; set; }

    public DateTime? SprintStartDate { get; set; }
    public TimeSpan? EveningStandUpTime { get; set; }
    public TimeSpan? MessageStartTime { get; set; }
    public TimeSpan? MessageEndTime { get; set; }
    public int? ReflectionDay { get; set; }
    public string? SheetId { get; set; }
}
namespace Neurointegration.Api.DataModels.Dto;

public record CreateUser
{
    public long UserId { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public bool IAmCoach { get; set; }
    public bool SendRegularMessages { get; set; }
    
    public TimeSpan? EveningStandUpTime { get; set; }
    public TimeSpan? MessageStartTime { get; set; }
    public TimeSpan? MessageEndTime { get; set; }
    public DateTime? FirstReflectionDate { get; set; }
    public DateTime? SprintStartDate { get; set; }
}
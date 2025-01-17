namespace Neurointegration.Api.DataModels.Dto;

public record UpdateUser
{
    public long UserId { get; set; }
    public string? Email { get; set; }
    public bool? IAmCoach { get; set; }
    public bool? SendRegularMessages { get; set; }
    public TimeSpan? EveningStandUpTime { get; set; }
    public TimeSpan? WeekReflectionTime { get; set; }
    public TimeSpan? MessageStartTime { get; set; }
    public TimeSpan? MessageEndTime { get; set; }
    public DateTime? ReflectionDate { get; set; }
    public DateTime? SprintStartDate { get; set; }
}
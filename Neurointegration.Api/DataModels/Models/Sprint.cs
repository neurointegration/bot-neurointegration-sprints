namespace Neurointegration.Api.DataModels.Models;

public record Sprint
{
    public Sprint()
    {
    }

    public long SprintNumber { get; set; }
    public long UserId { get; set; }
    public DateTime SprintStartDate { get; set; }
    public string SheetId { get; set; }
    public int LifeCount { get; set; }
    public int PleasureCount { get; set; }
    public int DriveCount { get; set; }

    public bool IsActive()
    {
        return SprintStartDate.AddDays(SprintConstants.SprintDaysCount - 1) >= DateTime.UtcNow.Date;
    }

    public int GetSprintWeek()
    {
        var dayOfSprint = DateOnly.FromDateTime(DateTime.UtcNow).DayNumber -
                          DateOnly.FromDateTime(SprintStartDate).DayNumber;
        return dayOfSprint / 7;
    }
}
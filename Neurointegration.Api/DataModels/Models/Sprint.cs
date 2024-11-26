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
}
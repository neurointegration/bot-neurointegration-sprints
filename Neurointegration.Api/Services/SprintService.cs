using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.DataModels.Result;
using Neurointegration.Api.Extensions;
using Neurointegration.Api.Storages;
using Neurointegration.Api.Storages.Sprints;

namespace Neurointegration.Api.Services;

public class SprintService : ISprintService
{
    private readonly IGoogleStorage googleStorage;
    private readonly ISprintStorage sprintStorage;

    public SprintService(IGoogleStorage googleStorage, ISprintStorage sprintStorage)
    {
        this.googleStorage = googleStorage;
        this.sprintStorage = sprintStorage;
    }

    public async Task<Sprint> CreateSprint(
        User user,
        long sprintNumber,
        DateTime sprintStartDate)
    {
        var sheetId = await googleStorage.CreateSheet(DateOnly.FromDateTime(sprintStartDate));
        await googleStorage.GrantedAccessSheet(sheetId, user.Email);
        var sprint = new Sprint
        {
            UserId = user.UserId,
            SprintNumber = sprintNumber,
            SheetId = sheetId,
            SprintStartDate = sprintStartDate
        };
        await sprintStorage.SaveOrUpdate(sprint);

        return sprint;
    }

    public async Task SaveOrUpdate(Sprint sprint)
    {
        await sprintStorage.SaveOrUpdate(sprint);
    }

    public async Task<(Sprint?, long)> GetActiveSprint(long userId)
    {
        var sprints = await sprintStorage.GetUserSprints(userId);
        var lastSprint = sprints.MaxBy(sprint => sprint.SprintNumber);
        if (lastSprint.SprintStartDate.AddDays(27) >= DateTime.UtcNow.Date)
            return (lastSprint, lastSprint.SprintNumber);

        return (null, lastSprint.SprintNumber);
    }

    public async Task<List<string>> GetUserGoogleSheets(long ownerId)
    {
        return await sprintStorage.GetUserGoogleSheets(ownerId);
    }

    public async Task<Result<Sprint>> GetSprint(long userId, long sprintNumber)
    {
        return await sprintStorage.GetSprint(userId, sprintNumber);
    }

    public async Task<List<Sprint>> GetSprints(long ownerId)
    {
        return await sprintStorage.GetUserSprints(ownerId);
    }
}
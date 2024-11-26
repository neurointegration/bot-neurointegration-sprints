using Neurointegration.Api.DataModels.Models;

namespace Neurointegration.Api.Storages.Sprints;

public interface ISprintStorage
{
    Task<List<string>> GetUserGoogleSheets(long userId);
    Task<List<Sprint>> GetUserSprints(long userId);
    Task SaveOrUpdate(Sprint sprint);
    Task<Sprint?> GetSprint(long userId, long sprintNumber);
}
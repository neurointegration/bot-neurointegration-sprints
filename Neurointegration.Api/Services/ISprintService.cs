using Neurointegration.Api.DataModels.Models;

namespace Neurointegration.Api.Services;

public interface ISprintService
{
    Task<Sprint> CreateSprint(
        User user,
        long sprintNumber,
        DateTime sprintStartDate);
    
    Task SaveOrUpdate(Sprint sprint);
    
    Task<(Sprint?, long)> GetActiveSprint(long userId);
    Task<List<string>> GetUserGoogleSheets(long ownerId);
    Task<Sprint?> GetSprint(long userId, long sprintNumber);
    Task<List<Sprint>> GetSprints(long ownerId);
}
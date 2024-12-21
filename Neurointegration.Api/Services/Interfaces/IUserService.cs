using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Models;

namespace Neurointegration.Api.Services;

public interface IUserService
{
    Task<User> CreateUser(CreateUser createUser);
    Task<User> GetUser(long userId);
    Task GrantedAccess(long ownerId, long grantedUserId);
    Task<List<string>> GetSpreadSheets(long userId);
    Task<bool> HaveAccess(long grantedUserId, long ownerId);
    Task<List<User>> GetPublicCoachs();
    Task<User> UpdateUser(UpdateUser updateUser);
    Task<List<User>> GetStudents(long userId);
    Task DeleteAccess(long ownerId, long grantedUserId);
    Task UpdateAccess(long userId, string sheetId);
    Task<List<Sprint>> GetSprints(long ownerId);
}
using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.DataModels.Result;

namespace Neurointegration.Api.Storages;

public interface IUsersStorage
{
    Task SaveUser(User user);
    Task<Result<User>> GetUser(long userId);
    Task<Result<User>> GetUser(string username);
    Task AddAccess(long grantedUserId, long ownerUserId, string sheetId, string permissionId);
    Task<IEnumerable<long>> GetAccessUsers(long userId);
    Task<List<User>> GetPublicCoachs();
    Task<List<SheetPermission>> GetPermissions(long grantedUserId, long ownerId);
    Task DeleteAccess(long grantedUserId, long ownerId);
    Task<List<long>> GetGrantedUsers(long userId);
}
using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Result;

namespace Neurointegration.Api.Storages.User;

public interface IUsersStorage
{
    Task SaveUser(DataModels.Models.User user);
    Task<Result<DataModels.Models.User>> GetUser(long userId);
    Task<Result<DataModels.Models.User>> GetUser(string username);
    Task AddAccess(long grantedUserId, long ownerUserId, string sheetId, string permissionId);
    Task<IEnumerable<long>> GetAccessUsers(long userId);
    Task<List<DataModels.Models.User>> GetPublicCoachs();
    Task<List<SheetPermission>> GetPermissions(long grantedUserId, long ownerId);
    Task DeleteAccess(long grantedUserId, long ownerId);
    Task<List<long>> GetGrantedUsers(long userId);
    Task DeleteUser(long userId);
}
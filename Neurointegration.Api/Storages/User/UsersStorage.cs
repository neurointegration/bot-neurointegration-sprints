using Common.Ydb;
using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.DataModels.Result;
using Neurointegration.Api.Excpetions;
using Neurointegration.Api.Settings;
using Neurointegration.Api.Storages.Mapper;
using Ydb.Sdk.Value;

namespace Neurointegration.Api.Storages;

public class UsersStorage : IUsersStorage
{
    private readonly YdbClient ydbClient;
    private readonly UserMapper userMapper;

    public UsersStorage(YdbClient ydbClient, UserMapper userMapper)
    {
        this.ydbClient = ydbClient;
        this.userMapper = userMapper;
    }

    public async Task SaveUser(User user)
    {
        await ydbClient.ExecuteModify($@"
             DECLARE ${UserDbSettings.UserIdField} AS Int64;
             DECLARE ${UserDbSettings.EmailField} AS text;
             DECLARE ${UserDbSettings.UsernameField} AS text;
             DECLARE ${UserDbSettings.MessageStartTimeField} AS Interval?;
             DECLARE ${UserDbSettings.MessageEndTimeField} AS Interval?;
             DECLARE ${UserDbSettings.EveningStandUpTimeField} AS Interval?;
             DECLARE ${UserDbSettings.WeekReflectionTime} AS Interval?;
             DECLARE ${UserDbSettings.IAmCoachField} AS Bool;
             DECLARE ${UserDbSettings.SendRegularMessagesField} AS Bool;

             REPLACE INTO {UserDbSettings.TableName} 
                ( {UserDbSettings.UserIdField}, {UserDbSettings.EmailField}, {UserDbSettings.UsernameField}, {UserDbSettings.MessageStartTimeField}, {UserDbSettings.MessageEndTimeField},
                  {UserDbSettings.EveningStandUpTimeField}, {UserDbSettings.WeekReflectionTime}, {UserDbSettings.IAmCoachField}, {UserDbSettings.SendRegularMessagesField} )
             VALUES ( ${UserDbSettings.UserIdField}, ${UserDbSettings.EmailField}, ${UserDbSettings.UsernameField}, ${UserDbSettings.MessageStartTimeField}, ${UserDbSettings.MessageEndTimeField},
                  ${UserDbSettings.EveningStandUpTimeField}, ${UserDbSettings.WeekReflectionTime}, ${UserDbSettings.IAmCoachField}, ${UserDbSettings.SendRegularMessagesField} )",
            new Dictionary<string, YdbValue>()
            {
                {$"${UserDbSettings.UserIdField}", YdbValue.MakeInt64(user.UserId)},
                {$"${UserDbSettings.EmailField}", YdbValue.MakeUtf8(user.Email)},
                {$"${UserDbSettings.UsernameField}", YdbValue.MakeUtf8(user.Username)},
                {$"${UserDbSettings.MessageStartTimeField}", YdbValue.MakeOptionalInterval(user.MessageStartTime)},
                {$"${UserDbSettings.MessageEndTimeField}", YdbValue.MakeOptionalInterval(user.MessageEndTime)},
                {$"${UserDbSettings.EveningStandUpTimeField}", YdbValue.MakeOptionalInterval(user.EveningStandUpTime)},
                {$"${UserDbSettings.WeekReflectionTime}", YdbValue.MakeOptionalInterval(user.WeekReflectionTime)},
                {$"${UserDbSettings.IAmCoachField}", YdbValue.MakeBool(user.IAmCoach)},
                {$"${UserDbSettings.SendRegularMessagesField}", YdbValue.MakeBool(user.SendRegularMessages)},
            });
    }

    public async Task<Result<User>> GetUser(long userId)
    {
        var rows = await ydbClient.ExecuteFind($@"
            DECLARE ${UserDbSettings.UserIdField} AS Int64;

            SELECT *
            FROM {UserDbSettings.TableName}
            WHERE {UserDbSettings.UserIdField} = ${UserDbSettings.UserIdField}
        ", new Dictionary<string, YdbValue>
        {
            {$"${UserDbSettings.UserIdField}", YdbValue.MakeInt64(userId)}
        });

        var rowsList = rows.ToList();
        if (rowsList.Count == 0)
            return Result<User>.Fail(Error.NotFound($"Пользователь с userId={userId} не найден"));

        return Result<User>.Success(userMapper.ToUserEntity(rowsList[0]));
    }

    public async Task<Result<User>> GetUser(string username)
    {
        var rows = await ydbClient.ExecuteFind($@"
            DECLARE ${UserDbSettings.UsernameField} AS Utf8;

            SELECT *
            FROM {UserDbSettings.TableName}
            WHERE {UserDbSettings.UsernameField} = ${UserDbSettings.UsernameField}
        ", new Dictionary<string, YdbValue>
        {
            {$"${UserDbSettings.UsernameField}", YdbValue.MakeUtf8(username)}
        });

        var rowsList = rows.ToList();
        if (rowsList.Count == 0)
            return Result<User>.Fail(Error.NotFound($"Пользователь с username={username} не найден"));

        return Result<User>.Success(userMapper.ToUserEntity(rowsList[0]));
    }

    public async Task AddAccess(long grantedUserId, long ownerUserId, string sheetId, string permissionId)
    {
        await ydbClient.ExecuteModify($@"
             DECLARE ${UserAccessDbSettings.GrantedUserIdField} AS Int64;
             DECLARE ${UserAccessDbSettings.OwnerUserIdField} AS Int64;
             DECLARE ${UserAccessDbSettings.PermissionIdField} AS Utf8;
             DECLARE ${UserAccessDbSettings.SheetIdField} AS Utf8;

             REPLACE INTO {UserAccessDbSettings.TableName} 
                ( {UserAccessDbSettings.GrantedUserIdField}, {UserAccessDbSettings.OwnerUserIdField},
                   {UserAccessDbSettings.PermissionIdField}, {UserAccessDbSettings.SheetIdField} )
             VALUES ( ${UserAccessDbSettings.GrantedUserIdField}, ${UserAccessDbSettings.OwnerUserIdField},
                        ${UserAccessDbSettings.PermissionIdField}, ${UserAccessDbSettings.SheetIdField} )",
            new Dictionary<string, YdbValue>()
            {
                {$"${UserAccessDbSettings.GrantedUserIdField}", YdbValue.MakeInt64(grantedUserId)},
                {$"${UserAccessDbSettings.OwnerUserIdField}", YdbValue.MakeInt64(ownerUserId)},
                {$"${UserAccessDbSettings.PermissionIdField}", YdbValue.MakeUtf8(permissionId)},
                {$"${UserAccessDbSettings.SheetIdField}", YdbValue.MakeUtf8(sheetId)}
            });
    }

    public async Task<List<SheetPermission>> GetPermissions(long grantedUserId, long ownerUserId)
    {
        var rows = await ydbClient.ExecuteFind($@"
             DECLARE ${UserAccessDbSettings.GrantedUserIdField} AS Int64;
             DECLARE ${UserAccessDbSettings.OwnerUserIdField} AS Int64;

            SELECT {UserAccessDbSettings.PermissionIdField}, {UserAccessDbSettings.SheetIdField}
            FROM {UserAccessDbSettings.TableName}
            WHERE {UserAccessDbSettings.GrantedUserIdField} = ${UserAccessDbSettings.GrantedUserIdField} AND
                    {UserAccessDbSettings.OwnerUserIdField} = ${UserAccessDbSettings.OwnerUserIdField}
        ", new Dictionary<string, YdbValue>()
        {
            {$"${UserAccessDbSettings.GrantedUserIdField}", YdbValue.MakeInt64(grantedUserId)},
            {$"${UserAccessDbSettings.OwnerUserIdField}", YdbValue.MakeInt64(ownerUserId)}
        });

        return rows.Select(row => userMapper.ToSheetPermissionEntity(row)).ToList();
    }

    public async Task DeleteAccess(long grantedUserId, long ownerId)
    {
        await ydbClient.ExecuteModify($@"
             DECLARE ${UserAccessDbSettings.GrantedUserIdField} AS Int64;
             DECLARE ${UserAccessDbSettings.OwnerUserIdField} AS Int64;

            DELETE FROM {UserAccessDbSettings.TableName}
            WHERE {UserAccessDbSettings.GrantedUserIdField} == ${UserAccessDbSettings.GrantedUserIdField} AND 
                  {UserAccessDbSettings.OwnerUserIdField} == ${UserAccessDbSettings.OwnerUserIdField};",
            new Dictionary<string, YdbValue>()
            {
                {$"${UserAccessDbSettings.GrantedUserIdField}", YdbValue.MakeInt64(grantedUserId)},
                {$"${UserAccessDbSettings.OwnerUserIdField}", YdbValue.MakeInt64(ownerId)}
            });
    }

    public async Task<List<long>> GetGrantedUsers(long userId)
    {
        var rows = await ydbClient.ExecuteFind($@"
            DECLARE ${UserAccessDbSettings.OwnerUserIdField} AS Int64;

            SELECT DISTINCT {UserAccessDbSettings.GrantedUserIdField}
            FROM {UserAccessDbSettings.TableName}
            WHERE {UserAccessDbSettings.OwnerUserIdField} = ${UserAccessDbSettings.OwnerUserIdField}
        ", new Dictionary<string, YdbValue>
        {
            {$"${UserAccessDbSettings.OwnerUserIdField}", YdbValue.MakeInt64(userId)}
        });

        return rows.Select(row => row[UserAccessDbSettings.GrantedUserIdField].GetInt64()).ToList();
    }

    public async Task<IEnumerable<long>> GetAccessUsers(long userId)
    {
        var rows = await ydbClient.ExecuteFind($@"
            DECLARE ${UserAccessDbSettings.GrantedUserIdField} AS Int64;

            SELECT {UserAccessDbSettings.OwnerUserIdField}
            FROM {UserAccessDbSettings.TableName}
            WHERE {UserAccessDbSettings.GrantedUserIdField} = ${UserAccessDbSettings.GrantedUserIdField}
        ", new Dictionary<string, YdbValue>
        {
            {$"${UserAccessDbSettings.GrantedUserIdField}", YdbValue.MakeInt64(userId)}
        });

        return rows.Select(row => row[UserAccessDbSettings.OwnerUserIdField].GetInt64());
    }

    public async Task<List<User>> GetPublicCoachs()
    {
        var rows = await ydbClient.ExecuteFind($@"
            SELECT *
            FROM {UserDbSettings.TableName}
            WHERE {UserDbSettings.IAmCoachField} = True
        ", new Dictionary<string, YdbValue>());

        return rows.Select(row => userMapper.ToUserEntity(row)).ToList();
    }
}
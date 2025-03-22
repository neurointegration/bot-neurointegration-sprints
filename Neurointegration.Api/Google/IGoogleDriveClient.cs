namespace Neurointegration.Api.Google;

public interface IGoogleDriveClient
{
    Task<string> AddUserToSpreadSheet(string spreadSheetId, string email);

    Task<string> CreateSpreadSheet();

    Task DeleteUserFromSpreadSheet(string sheetId, string permissionId);
}
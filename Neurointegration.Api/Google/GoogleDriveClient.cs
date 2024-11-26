using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Neurointegration.Api.Settings;
using File = Google.Apis.Drive.v3.Data.File;

namespace Neurointegration.Api.Google;

public class GoogleDriveClient
{
    private readonly DriveService driveService;

    public GoogleDriveClient(ApiSecretSettings secretSettings)
    {
        var credential = GoogleCredential
            .FromJson(secretSettings.GoogleSheetsApiKey)
            .CreateScoped(DriveService.Scope.DriveFile);

        driveService = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential
        });
    }

    public async Task<string> AddUserToSpreadSheet(string spreadSheetId, string email)
    {
        var permission = new Permission()
        {
            EmailAddress = email,
            Role = "writer",
            Type = "user"
        };

        var createdPermission = await driveService.Permissions.Create(permission, spreadSheetId).ExecuteAsync();
        return createdPermission.Id;
    }

    public async Task<string> CreateSpreadSheet()
    {
        var id = "1bVWMvBdJ7hgMszqOw1Rzo4C-IfVapm_mtQ0ThH1Hmhs";
        var file = new File()
        {
            Name = "Текущий спринт"
        };
        var spreadsheet = await driveService.Files.Copy(file, id).ExecuteAsync();
        if (spreadsheet == null)
            throw new Exception("Google Api not create spreadsheet");

        return spreadsheet.Id;
    }

    public async Task DeleteUserFromSpreadSheet(string sheetId, string permissionId)
    {
        await driveService.Permissions.Delete(sheetId, permissionId).ExecuteAsync();
    }
}
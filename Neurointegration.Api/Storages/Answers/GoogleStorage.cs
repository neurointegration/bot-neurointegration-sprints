using Neurointegration.Api.Extensions;
using Neurointegration.Api.Google;

namespace Neurointegration.Api.Storages.Answers;

public class GoogleStorage : IGoogleStorage
{
    private readonly GoogleSheetClient sheetClient;
    private readonly GoogleDriveClient driveClient;
    private readonly GoogleSheetUtils googleSheetUtils;

    public GoogleStorage(GoogleSheetClient sheetClient, GoogleDriveClient driveClient,
        GoogleSheetUtils googleSheetUtils)
    {
        this.sheetClient = sheetClient;
        this.driveClient = driveClient;
        this.googleSheetUtils = googleSheetUtils;
    }

    public async Task Save(string answer, string sheetId, string range)
    {
        await sheetClient.Write(sheetId, range, answer);
    }

    public async Task<string> CreateSheet(DateOnly startDate)
    {
        var spreadsheetId = await driveClient.CreateSpreadSheet();
        await SetStatusDates(spreadsheetId, startDate);

        Console.WriteLine(spreadsheetId);
        return spreadsheetId;
    }

    public async Task<string> GrantedAccessSheet(string sheetId, string email)
    {
        return await driveClient.AddUserToSpreadSheet(sheetId, email);
    }

    public async Task DeleteAccessSheets(string sheetId, string permissionId)
    {
        await driveClient.DeleteUserFromSpreadSheet(sheetId, permissionId);
    }
    
    private async Task SetStatusDates(string spreadsheetId, DateOnly startDate)
    {
        for (var i = 0; i < 28; i++)
        {
            await sheetClient.Write(
                spreadsheetId,
                googleSheetUtils.GetDateStatusCell(i),
                startDate.AddDays(i).ToGoogleDateString());
        }
    }
}
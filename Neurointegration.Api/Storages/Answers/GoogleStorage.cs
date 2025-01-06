using Neurointegration.Api.Extensions;
using Neurointegration.Api.Google;

namespace Neurointegration.Api.Storages.Answers;

public class GoogleStorage : IGoogleStorage
{
    private readonly IGoogleSheetClient sheetClient;
    private readonly IGoogleDriveClient driveClient;
    private readonly GoogleSheetUtils googleSheetUtils;
    private readonly ILogger log;

    public GoogleStorage(IGoogleSheetClient sheetClient, IGoogleDriveClient driveClient,
        GoogleSheetUtils googleSheetUtils, ILogger log)
    {
        this.sheetClient = sheetClient;
        this.driveClient = driveClient;
        this.googleSheetUtils = googleSheetUtils;
        this.log = log;
    }

    public Task Save(string answer, string sheetId, string range)
    {
        log.LogInformation($"Сохранение в гугл таблицу. SheetId={sheetId}, range={range}");
        Task.Run(() => sheetClient.Write(sheetId, range, answer));
        log.LogInformation($"Сохранили ответ в гугл таблицу");
        return Task.CompletedTask;
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
        for (var i = 0; i < SprintConstants.SprintDaysCount; i++)
        {
            await sheetClient.Write(
                spreadsheetId,
                googleSheetUtils.GetDateStatusCell(i),
                startDate.AddDays(i).ToGoogleDateString());
        }
    }
}
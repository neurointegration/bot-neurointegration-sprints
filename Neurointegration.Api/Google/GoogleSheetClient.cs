using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Neurointegration.Api.Settings;

namespace Neurointegration.Api.Google;

public class GoogleSheetClient
{
    private readonly SheetsService sheetsService;

    public GoogleSheetClient(ApiSecretSettings secretSettings)
    {
        var credential = GoogleCredential
            .FromJson(secretSettings.GoogleSheetsApiKey)
            .CreateScoped(SheetsService.Scope.Spreadsheets);

        sheetsService = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential
        });
    }

    public async Task<IList<IList<object>>> Read(string spreadsheetId, string range)
    {
        var request =
            sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);

        var response = await request.ExecuteAsync();

        return response.Values;
    }

    public async Task Write(string spreadsheetId, string range, string value)
    {
        var body = new ValueRange()
        {
            Range = range,
            Values = new List<IList<object>>()
            {
                new List<object>()
                {
                    value
                }
            }
        };

        var request =
            sheetsService.Spreadsheets.Values.Update(body, spreadsheetId, range);
        request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
        var response = await request.ExecuteAsync();
    }
}
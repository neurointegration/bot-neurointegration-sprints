namespace Neurointegration.Api.Google;

public interface IGoogleSheetClient
{
    Task<IList<IList<object>>> Read(string spreadsheetId, string range);

    Task Write(string spreadsheetId, string range, string value);
}
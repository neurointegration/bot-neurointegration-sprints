namespace Neurointegration.Api.Storages;

public interface IGoogleStorage
{
    Task Save(string text, string sheetId, string range);
    Task<string> CreateSheet(DateOnly startDate);
    Task<string> GrantedAccessSheet(string sheetId, string email);
    Task DeleteAccessSheets(string sheetId, string permissionId);
}
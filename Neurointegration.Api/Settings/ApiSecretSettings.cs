using Common.Ydb;
using Newtonsoft.Json;

namespace Neurointegration.Api.Settings;

public record ApiSecretSettings
{
    public string GoogleSheetsApiKey { get; set; }
    public YdbSecretSettings YdbSecretSettings { get; set; }
    
    public static ApiSecretSettings FromJson(string path)
    {
        var text = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<ApiSecretSettings>(text) ?? throw new ArgumentException("Не заданы секреты");
    }

    public static ApiSecretSettings FromEnvironment()
    {
        return new ApiSecretSettings()
        {
            GoogleSheetsApiKey = Environment.GetEnvironmentVariable("GOOGLESHEETS_APIKEY") ??
                                 throw new ArgumentException("Не задана переменная GOOGLESHEETS_APIKEY"),
            
            YdbSecretSettings = YdbSecretSettings.FromEnvironment()
        };
    }
}
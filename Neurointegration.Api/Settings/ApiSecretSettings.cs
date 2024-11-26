namespace Neurointegration.Api.Settings;

public record ApiSecretSettings
{
    public string GoogleSheetsApiKey { get; set; }
    public string YdbEndpoint { get; set; }
    public string YdbPath { get; set; }
    public string? IamTokenPath { get; set; }
    public string? TokenJson { get; set; }


    public static ApiSecretSettings FromConfiguration(IConfigurationRoot configuration)
    {
        return new ApiSecretSettings()
        {
            GoogleSheetsApiKey = configuration.GetSection("Settings:GoogleSheets:ApiKey").Get<string>(),
            YdbEndpoint = configuration.GetSection("Settings:YDB:Endpoint").Get<string>(),
            YdbPath = configuration.GetSection("Settings:YDB:Path").Get<string>(),
            IamTokenPath = configuration.GetSection("Settings:YDB:Token").Get<string>(),
        };
    }

    public static ApiSecretSettings FromEnvironment()
    {
        return new ApiSecretSettings()
        {
            GoogleSheetsApiKey = Environment.GetEnvironmentVariable("GOOGLESHEETS_APIKEY") ??
                                 throw new ArgumentException("Не задана переменная GOOGLESHEETS_APIKEY"),
            
            YdbEndpoint = Environment.GetEnvironmentVariable("NEURO_YDB_ENDPOINT") ??
                          throw new ArgumentException("Не задана переменная NEURO_YDB_ENDPOINT"),
            
            YdbPath = Environment.GetEnvironmentVariable("NEURO_YDB_PATH") ??
                      throw new ArgumentException("Не задана переменная NEURO_YDB_PATH"),
            
            IamTokenPath = Environment.GetEnvironmentVariable("IAM_TOKEN_PATH"),
        };
    }
}
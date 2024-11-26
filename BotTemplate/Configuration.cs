using Microsoft.Extensions.Configuration;

namespace BotTemplate;

public record Configuration
{
    public string TelegramToken { get; set; }
    public string YdbEndpoint { get; set; }
    public string YdbPath { get; set; }
    public string? IamTokenPath { get; set; }
    public string? BackendApiKey { get; set; }
    public string? BackendBaseUrl { get; set; }
    public string? TriggerFrequencyMinutes { get; set; }

    private Configuration()
    {
    }

    public static Configuration FromJson(string path)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(path, optional: false, reloadOnChange: true)
            .Build();

        return new Configuration
        {
            TelegramToken = configuration.GetSection("AppSettings")[nameof(TelegramToken)] ??
                            throw new ArgumentException("Не задана переменная GOOGLESHEETS_APIKEY"),

            YdbEndpoint = configuration.GetSection("AppSettings")[nameof(YdbEndpoint)] ??
                          throw new ArgumentException("Не задана переменная NEURO_YDB_ENDPOINT"),

            YdbPath = configuration.GetSection("AppSettings")[nameof(YdbPath)] ??
                      throw new ArgumentException("Не задана переменная NEURO_YDB_PATH"),

            IamTokenPath = configuration.GetSection("AppSettings")[nameof(IamTokenPath)],
            
            BackendApiKey = configuration.GetSection("AppSettings")[nameof(BackendApiKey)],
            
            BackendBaseUrl = configuration.GetSection("AppSettings")[nameof(BackendBaseUrl)],
            
            TriggerFrequencyMinutes = configuration.GetSection("AppSettings")[nameof(TriggerFrequencyMinutes)],
        };
    }

    public static Configuration FromEnvironment()
    {
        return new Configuration()
        {
            TelegramToken = Environment.GetEnvironmentVariable("NEURO_TELEGRMA_TOKEN") ??
                            throw new ArgumentException("Не задана переменная GOOGLESHEETS_APIKEY"),

            YdbEndpoint = Environment.GetEnvironmentVariable("NEURO_YDB_ENDPOINT") ??
                          throw new ArgumentException("Не задана переменная NEURO_YDB_ENDPOINT"),

            YdbPath = Environment.GetEnvironmentVariable("NEURO_YDB_PATH") ??
                      throw new ArgumentException("Не задана переменная NEURO_YDB_PATH"),

            IamTokenPath = Environment.GetEnvironmentVariable("IAM_TOKEN_PATH"),
            BackendApiKey = Environment.GetEnvironmentVariable("NEURO_BACKEND_APIKEY"),
            BackendBaseUrl = Environment.GetEnvironmentVariable("NEURO_BACKEND_URL"),
            TriggerFrequencyMinutes = Environment.GetEnvironmentVariable("NEURO_TRIGGER_FREQUENCY_MINUTES"),
        };
    }
}
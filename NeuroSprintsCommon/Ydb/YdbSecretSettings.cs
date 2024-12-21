namespace Common.Ydb;

public class YdbSecretSettings
{
    public string YdbEndpoint { get; set; }
    public string YdbPath { get; set; }
    
    public string? IamTokenPath { get; set; }
    
    public static YdbSecretSettings FromEnvironment()
    {
        return new YdbSecretSettings()
        {
            YdbEndpoint = Environment.GetEnvironmentVariable("NEURO_YDB_ENDPOINT") ??
                          throw new ArgumentException("Не задана переменная NEURO_YDB_ENDPOINT"),
            
            YdbPath = Environment.GetEnvironmentVariable("NEURO_YDB_PATH") ??
                      throw new ArgumentException("Не задана переменная NEURO_YDB_PATH"),
            
            IamTokenPath = Environment.GetEnvironmentVariable("IAM_TOKEN_PATH"),
        };
    }
}
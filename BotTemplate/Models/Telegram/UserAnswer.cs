namespace BotTemplate.Models.Telegram;

public class UserAnswer
{
    public string Answer { get; private set; }
    public string Key { get; private set; }

    public UserAnswer(string answer, string key)
    {
        Answer = answer;
        Key = key;
    }
}
using BotTemplate.Models.Telegram;

namespace BotTemplate.Scenarios.RegularScenarios;

public static class WeekendReflectionMessages
{
    public static Message AskWhatIDoing()
    {
        return new Message("Что сделал по своим проектам на этой неделе?");
    }

    public static Message AskWhatINotDoing()
    {
        return new Message("А что не сделал по своим проектам на этой неделе?");
    }
    
    public static Message AskStatus()
    {
        return new Message("Что влияло на твое состояние на этой неделе? Как в позитивном, так и в негативном ключе");
    }
    
    public static Message AskOrbits()
    {
        return new Message("Каким образом в поведении на этой неделе проявлялись орбиты? Как выруливал?");
    }
    
    public static Message AskCorrection()
    {
        return new Message("Что изменишь на следующей неделе?");
    }
    
    public static Message FinishReflection()
    {
        return new Message("Желаю успехов в новой неделе!");
    }
    
    public static Message AskChanges()
    {
        return new Message("Какое главное изменение замечаешь?");
    }

    public static Message AskActions()
    {
        return new Message("Какие твои действия к этому привели?");
    }
    
    public static Message AskAbilities()
    {
        return new Message("Какие твои способности мне помогли?");
    }
    
    public static Message AskBeliefs()
    {
        return new Message("Как изменились твои убеждения о том, что возможно?");
    }
    
    public static Message AskSelfPerception()
    {
        return new Message("Как изменились твои убеждения о себе и отношения с собой?");
    }
    
    public static Message AskOpportunities()
    {
        return new Message("Какие возможности теперь для тебя доступны?");
    }
}
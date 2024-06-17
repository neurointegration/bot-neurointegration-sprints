using BotTemplate.Services.YDB;
using BotTemplate.Services.YDB.Repo;

namespace BotTemplate.Services.Telegram.Commands;

public class GetAnswersCommandHandler : IChatCommandHandler
{
    public string Command => "/get_all_answers";
    private readonly IRepo _repo;

    public GetAnswersCommandHandler(IRepo repo)
    {
        _repo = repo;
    }

    public async Task<string?> HandlePlainText(long fromChatId)
    {
        if (_repo is not UserAnswersRepo userAnswersRepo)
            throw new ArgumentException("Передан неверный тип репозитория");
        
        return string.Join('\n', (await userAnswersRepo.GetAllWithKeys(fromChatId)).Select(userAnswer => userAnswer.Answer));
    }
}
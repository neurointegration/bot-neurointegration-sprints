using BotTemplate.Services.YDB;
using BotTemplate.Services.YDB.Repo;

namespace BotTemplate.Services.Telegram.Commands;

public class GetAnswersCommandHandler : IChatCommandHandler
{
    public string Command => "/get_all_answers";
    private readonly IRepository repository;

    public GetAnswersCommandHandler(IRepository repository)
    {
        this.repository = repository;
    }

    public async Task<string?> HandlePlainText(long fromChatId)
    {
        if (repository is not UserAnswersRepository userAnswersRepo)
            throw new ArgumentException("Передан неверный тип репозитория");

        var answers = (await userAnswersRepo.GetAllWithKeys(fromChatId)).Select(userAnswer => userAnswer.Answer).ToList();
        return string.Join('\n', answers.Count == 0 ? "Тут ничего нет" : answers);
    }
}
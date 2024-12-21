using Neurointegration.Api.DataModels.Dto;

namespace Neurointegration.Api.Storages.Answers;

public interface IAnswerStorage
{
    Task Save(SendAnswer answer);
}
using Neurointegration.Api.DataModels.Dto;

namespace Neurointegration.Api.Services;

public interface IAnswersService
{
    Task Save(SendAnswer answer);
}
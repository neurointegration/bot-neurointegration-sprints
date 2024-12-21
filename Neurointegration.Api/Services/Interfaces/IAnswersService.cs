using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Result;

namespace Neurointegration.Api.Services;

public interface IAnswersService
{
    Task<Result> Save(SendAnswer answer);
}
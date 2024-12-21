using Microsoft.AspNetCore.Mvc;
using Neurointegration.Api.Excpetions;

namespace Neurointegration.Api.Extensions;

public static class ErrorExtensions
{
    public static IActionResult ToActionResult(this Error error)
    {
        return error.Status switch
        {
            ErrorStatus.NotFound => new ObjectResult(error) {StatusCode = 404},
            _ => new ObjectResult(error) {StatusCode = 500}
        };
    }
}
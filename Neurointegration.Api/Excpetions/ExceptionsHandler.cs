using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Neurointegration.Api.Excpetions;

public class ExceptionsHandler: IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        int statusCode;
        var message = context.Exception.Message;
        switch (context.Exception)
        {
            case ArgumentException:
                statusCode = 403;
                break;
            case NotFoundException:
                statusCode = 404;
                break;
            case DataConflictException:
            case ValidationException:
                statusCode = 409;
                break;
            case GoogleApiException:
                statusCode = 503;
                break;
            default:
                statusCode = 500;
                message = "Что-то пошло не так";
                break;
        }

        context.Result = new ContentResult
        {
            ContentType = "application/json",
            StatusCode = statusCode,
            Content = JsonSerializer.Serialize(new ResponseExceptionDto(statusCode, message))
        };

        context.ExceptionHandled = true;
    }
}
using Neurointegration.Api.DataModels.Models;

namespace Neurointegration.Api.Helpers;

public class QuestionHelper
{
    private readonly Random random = new();

    public DateTime GetNewStatusQuestionDate(Question? question, User user)
    {
        var reply = question.SprintReplyNumber + 1;
        var newReply = reply % 3;
        var time = GetNewStatusQuestionTime(question, user);

        var day = newReply == 0 ? question.Date.Date.AddDays(1) : question.Date.Date;
        return day + time;
    }


    public TimeSpan GetNewStatusQuestionTime(Question? question, User user)
    {
        var reply = question.SprintReplyNumber + 1;
        var dayReply = reply % 3;

        if (user.MessageEndTime < user.MessageStartTime)
            user.MessageEndTime += TimeSpan.FromDays(1);

        var period = (user.MessageEndTime - user.MessageStartTime) / 3;
        var startTime = user.MessageStartTime + period * dayReply;
        var value = startTime + TimeSpan.FromMinutes(random.Next((int) period.Value.TotalMinutes));
        if (value.Value.Days > 0)
            value = value.Value - TimeSpan.FromDays(value.Value.Days);

        return value.Value;
    }
}
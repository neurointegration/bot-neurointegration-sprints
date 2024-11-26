using FluentValidation;

namespace Neurointegration.Api.DataModels.Models;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(user => user.Email).EmailAddress();

        When(user => user.SendRegularMessages, () =>
        {
            RuleFor(user => user.MessageStartTime)
                .InclusiveBetween(TimeSpan.Parse("00:00:00"), TimeSpan.Parse("23:59:59"));
            
            RuleFor(user => user.MessageEndTime)
                .InclusiveBetween(TimeSpan.Parse("00:00:00"), TimeSpan.Parse("23:59:59"));
            
            RuleFor(user => user.EveningStandUpTime)
                .InclusiveBetween(TimeSpan.Parse("00:00:00"), TimeSpan.Parse("23:59:59"));
        });

    }
}
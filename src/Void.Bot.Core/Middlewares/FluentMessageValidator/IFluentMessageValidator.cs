using FluentValidation;
using FluentValidation.Results;
using Mediator.Net.Contracts;

namespace Void.Bot.Core.Middlewares.FluentMessageValidator;

public interface IFluentMessageValidator
{
    void ValidateMessage<TMessage>(TMessage message) where TMessage : IMessage;
    
    Type ForMessageType { get; }
}

public class FluentMessageValidator<T> : AbstractValidator<T>, IFluentMessageValidator where T : class
{
    public void ValidateMessage<TMessage>(TMessage message) where TMessage : IMessage
    {
        if (message is not T instance)
            return;
        
        var result = Validate(instance);
        if (!result.IsValid)
        {
            var validationErrors = new List<ValidationFailure>();
            result.Errors.ForEach(e => validationErrors.Add(new ValidationFailure(e.PropertyName, e.ErrorMessage)));

            if (validationErrors.Any())
            {
                throw new ValidationException(validationErrors);
            }
        }
    }

    public Type ForMessageType => typeof(T);
}
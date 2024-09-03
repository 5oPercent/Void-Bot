using AutoMapper;
using Void.Bot.Core.Domain;
using Void.Bot.Message.Commands.MessageLogging;
using Void.Bot.Message.Dto.MessageLogging;

namespace Void.Bot.Core.Mappings;

public class MessageLogMapping : Profile
{
    public MessageLogMapping()
    {
        CreateMap<MessageLog, MessageLogDto>().ReverseMap();
        CreateMap<MessageLog, AddMessageLoggingCommand>().ReverseMap();
    }
}
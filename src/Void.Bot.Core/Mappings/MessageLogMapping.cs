using AutoMapper;
using Void.Bot.Core.Domain;
using Void.Bot.Core.Extension;
using Void.Bot.Message.Commands.MessageLogging;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.MessageLogging;

namespace Void.Bot.Core.Mappings;

public class MessageLogMapping : Profile
{
    public MessageLogMapping()
    {
        CreateMap<MessageLog, MessageLogDto>().ReverseMap();
        CreateMap<MessageLog, AddMessageLoggingCommand>().ReverseMap();

        CreateMap<PagedList<MessageLog>, PagedListDto<MessageLogDto>>().ReverseMap();
    }
}
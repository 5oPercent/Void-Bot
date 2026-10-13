using AutoMapper;

namespace Void.Bot.Core.Mappings;

/// <summary>
/// 项目映射基类，在创建映射时设置默认深度，也覆盖 ConvertUsing 映射。
/// </summary>
public abstract class VoidBotMappingProfile : Profile
{
    private const int DefaultMaxDepth = 64;

    public new IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
    {
        return base.CreateMap<TSource, TDestination>()
            .MaxDepth(DefaultMaxDepth);
    }

    public new IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList)
    {
        return base.CreateMap<TSource, TDestination>(memberList)
            .MaxDepth(DefaultMaxDepth);
    }

    public new IMappingExpression CreateMap(Type sourceType, Type destinationType)
    {
        return base.CreateMap(sourceType, destinationType)
            .MaxDepth(DefaultMaxDepth);
    }

    public new IMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList)
    {
        return base.CreateMap(sourceType, destinationType, memberList)
            .MaxDepth(DefaultMaxDepth);
    }
}

using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Validators.ERA;

/// <summary>当前客户端载体参数契约；只校验数据，不执行目标、空间或效果业务。</summary>
public static class SkillCarrierParameterValidator
{
    private static readonly IReadOnlyDictionary<string,Action<IReadOnlyDictionary<string,double>>> Validators =
        new Dictionary<string,Action<IReadOnlyDictionary<string,double>>>(StringComparer.Ordinal)
        {
            ["self"]=_=>{},
            ["team"]=_=>{},
            ["entity"]=p=>Range(p,"distance",20,0,1000),
            ["area"]=p=>
            {
                Range(p,"radius",5,double.Epsilon,1000);Range(p,"duration",8,double.Epsilon,3600);
                Range(p,"interval",1,0.05,3600);Range(p,"distance",20,0,1000);
            }
        };
    private static void Range(IReadOnlyDictionary<string,double> parameters,string key,double fallback,double minimum,double maximum)
    {
        double value=parameters.TryGetValue(key,out var configured)?configured:fallback;
        if(!double.IsFinite(value) || value<minimum || value>maximum)throw new ArgumentException("载体参数无效："+key);
    }
    public static void Validate(SkillVariant variant,bool requireKnown)
    {
        if(Validators.TryGetValue(variant.CarrierId,out var validate)){validate(variant.Parameters);return;}
        if(requireKnown)throw new ArgumentException("当前客户端未支持载体："+variant.CarrierId);
    }
}

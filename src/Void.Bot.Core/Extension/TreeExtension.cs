using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Extension;

public static class TreeExtension
{
    public static List<TEntity> ConvertTree<TKey, TEntity>(this IEnumerable<TEntity> sourceList, TKey? rootPid = default)
        where TKey : notnull
        where TEntity : class, ITree<TKey, TEntity>
    {
        var tree = new List<TEntity>();

        var list = sourceList.ToList();
        var dict = list.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.First());

        foreach (var entity in list)
        {
            if (Equals(entity.Pid, rootPid))
            {
                tree.Add(entity);
                continue;
            }
            
            if (dict.TryGetValue(entity.Pid, out var parent))
            {
                if (ReferenceEquals(parent, entity))
                {
                    tree.Add(entity); 
                    continue;
                }
                
                parent.Children ??= new List<TEntity>();
                parent.Children.Add(entity);
            }
            else
            {
                tree.Add(entity);
            }
        }
        
        return tree;
    }
}
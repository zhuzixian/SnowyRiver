using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SnowyRiver.Commons;
public static class Extensions
{
    public static void InsertPrevious<T>(this Collection<T> source, T? nullableSelectedItem)
        where T : IHasSortId, new()
    {
        if (nullableSelectedItem != null)
        {
            var selectedIndex = source.IndexOf(nullableSelectedItem);
            source.Insert(selectedIndex, new T());
            for (var i = selectedIndex; i < source.Count; i++)
            {
                if (source[i] is { } t)
                {
                    t.SortId = i + 1;
                }
            }
        }
    }

    public static void InsertNext<T>(this Collection<T> source, T? nullableSelectedItem)
        where T : IHasSortId, new()
    {
        if (nullableSelectedItem != null)
        {
            var selectedIndex = source.IndexOf(nullableSelectedItem);
            source.Insert(selectedIndex + 1, new T());
            for (var i = selectedIndex; i < source.Count; i++)
            {
                if (source[i] is { } t)
                {
                    t.SortId = i + 1;
                }
            }
        }
    }

    public static void Remove<T>(this Collection<T> source, T? nullableSelectedItem)
        where T : IHasSortId, new()
    {
        if (nullableSelectedItem is { } selectedStep)
        {
            var selectedIndex = source.IndexOf(selectedStep);
            source.RemoveAt(selectedIndex);
            for (var i = selectedIndex; i < source.Count; i++)
            {
                if (source[i] is { } t)
                {
                    t.SortId = i + 1;
                }
            }
        }
    }
}

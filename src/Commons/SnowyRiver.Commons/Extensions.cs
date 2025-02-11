using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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

    public static void FillSortId<T>(this IEnumerable<T> records, IQueryFilter filter)
        where T:IHasSortId
    {
        var recordsArray = records as T[] ?? records.ToArray();
        var baseSortId = (filter.PageIndex - 1) * filter.PageSize;
        for (var i = 0; i < recordsArray.Length; i++)
        {
            recordsArray[i].SortId = baseSortId + i + 1;
        }
    }

    public static bool IsNullOrEmpty(this string value)
    {
        return string.IsNullOrEmpty(value);
    }

    public static bool IsNullOrWhiteSpace(this string value)
    {
        return string.IsNullOrWhiteSpace(value);
    }
}

using System.Collections.ObjectModel;
using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Domain.Shared.Extensions;
public static class CollectionExtensions
{
    extension<T>(Collection<T> source) where T : IHasSortId, new()
    {
        public void InsertPreviousAndUpdateSortId(T? nullableSelectedItem)
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

        public void InsertNextAndUpdateSortId(T? nullableSelectedItem)
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

        public void AddAndUpdateSortId(T? nullableNewItem)
        {
            if (nullableNewItem is {} newItem)
            {
                newItem.SortId = source.Any()
                    ?  source.Max(x => x.SortId) + 1
                    : 1;
                source.Add(newItem);
            }
        }

        public void RemoveAndUpdateSortId(T? nullableSelectedItem)
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

    public static void FillSortId<T>(this IEnumerable<T> records, IQueryFilter? filter = null)
        where T:IHasSortId
    {
        var recordsArray = records as T[] ?? records.ToArray();
        var baseSortId = filter == null 
            ? 1
            : (filter.PageIndex - 1) * filter.PageSize;
        for (var i = 0; i < recordsArray.Length; i++)
        {
            recordsArray[i].SortId = baseSortId + i + 1;
        }
    }
}

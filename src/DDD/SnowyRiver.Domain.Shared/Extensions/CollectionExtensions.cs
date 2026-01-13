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

    extension<T>(IEnumerable<T> records) where T:IHasSortId
    {
        public void FillSortId(IQueryFilter? filter)
        {
            var startSortId = filter == null 
                ? 1
                : (filter.PageIndex - 1) * filter.PageSize;
            records.FillSortId(startSortId);
        }

        public void FillSortId(int startId = 1)
        {
            var recordsArray = records as T[] ?? records.ToArray();
            for (var i = 0; i < recordsArray.Length; i++)
            {
                recordsArray[i].SortId = startId + i;
            }
        }
    }
}

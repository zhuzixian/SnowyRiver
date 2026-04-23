using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SnowyRiver.ComponentModel.NotifyPropertyChanged;

public class ObservableCollectionEx<T> : ObservableCollection<T>
{
    private bool _isBulkOperation;
    private readonly List<NotifyCollectionChangedEventArgs> _pendingEvents = [];

    #region 批量操作控制

    /// <summary>
    /// 开始批量操作，在此期间不会立即触发CollectionChanged事件
    /// </summary>
    public IDisposable BeginBulkOperation()
    {
        _isBulkOperation = true;
        _pendingEvents.Clear();
        return new BulkOperationDisposable(this);
    }

    /// <summary>
    /// 结束批量操作，触发一次合并的事件
    /// </summary>
    public void EndBulkOperation()
    {
        _isBulkOperation = false;
        FlushPendingEvents();
    }

    private void FlushPendingEvents()
    {
        if (_pendingEvents.Count == 0)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            return;
        }

        // 如果有多个不同类型的操作，使用Reset事件
        if (_pendingEvents.Any(e => e.Action == NotifyCollectionChangedAction.Reset) ||
            _pendingEvents.Count > 1)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        else
        {
            // 只有一个操作，使用原事件
            OnCollectionChanged(_pendingEvents[0]);
        }

        _pendingEvents.Clear();
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (_isBulkOperation)
        {
            _pendingEvents.Add(e);
        }
        else
        {
            base.OnCollectionChanged(e);
        }
    }

    #endregion

    #region 批量操作方法

    /// <summary>
    /// 批量添加项目
    /// </summary>
    public void AddRange(IEnumerable<T> items)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));

        using (BeginBulkOperation())
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }
    }

    /// <summary>
    /// 批量插入项目
    /// </summary>
    public void InsertRange(int startIndex, IEnumerable<T> items)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));
        if (startIndex < 0 || startIndex > Count)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        using (BeginBulkOperation())
        {
            var index = startIndex;
            foreach (var item in items)
            {
                Insert(index, item);
                index++;
            }
        }
    }

    /// <summary>
    /// 批量移除指定的项目
    /// </summary>
    public int RemoveRange(IEnumerable<T> items)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));

        var removedCount = 0;
        using (BeginBulkOperation())
        {
            removedCount += items.ToList().Count(Remove);
        }
        return removedCount;
    }

    /// <summary>
    /// 按条件移除项目
    /// </summary>
    public int RemoveAll(Func<T, bool> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        var itemsToRemove = this.Where(predicate).ToList();
        return RemoveRange(itemsToRemove);
    }

    /// <summary>
    /// 按索引范围移除项目
    /// </summary>
    public int RemoveRange(int startIndex, int count)
    {
        if (startIndex < 0 || startIndex >= Count)
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        if (count < 0 || startIndex + count > Count)
            throw new ArgumentOutOfRangeException(nameof(count));

        var removedCount = 0;
        using (BeginBulkOperation())
        {
            for (var i = 0; i < count; i++)
            {
                RemoveAt(startIndex);
                removedCount++;
            }
        }
        return removedCount;
    }

    /// <summary>
    /// 按条件更新项目
    /// </summary>
    public int UpdateAll(Func<T, bool> predicate, Action<T> updateAction)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (updateAction == null) throw new ArgumentNullException(nameof(updateAction));

        var itemsToUpdate = this.Where(predicate).ToList();
        var updatedCount = 0;

        using (BeginBulkOperation())
        {
            foreach (var item in itemsToUpdate)
            {
                updateAction(item);
                updatedCount++;

                // 触发属性更改通知
                var index = IndexOf(item);
                if (index >= 0)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace, item, item, index));
                }
            }
        }

        return updatedCount;
    }

    /// <summary>
    /// 按条件替换项目
    /// </summary>
    public int ReplaceAll(Func<T, bool> predicate, Func<T, T> replacement)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (replacement == null) throw new ArgumentNullException(nameof(replacement));

        var itemsToReplace = this.Where(predicate).ToList();
        var replacedCount = 0;

        using (BeginBulkOperation())
        {
            foreach (var oldItem in itemsToReplace)
            {
                var index = IndexOf(oldItem);
                if (index >= 0)
                {
                    var newItem = replacement(oldItem);
                    this[index] = newItem;
                    replacedCount++;
                }
            }
        }

        return replacedCount;
    }

    /// <summary>
    /// 按条件查找项目
    /// </summary>
    public List<T> FindAll(Func<T, bool> predicate)
    {
        return predicate == null ? throw new ArgumentNullException(nameof(predicate)) : this.Where(predicate).ToList();
    }

    /// <summary>
    /// 按条件查找第一个匹配的项目
    /// </summary>
    public T? FindFirst(Func<T, bool> predicate)
    {
        return predicate == null ? throw new ArgumentNullException(nameof(predicate)) : this.FirstOrDefault(predicate);
    }

    /// <summary>
    /// 判断是否存在满足条件的项目
    /// </summary>
    public bool Exists(Func<T, bool> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        return this.Any(predicate);
    }

    /// <summary>
    /// 统计满足条件的项目数量
    /// </summary>
    public int CountBy(Func<T, bool> predicate)
    {
        return predicate == null ? throw new ArgumentNullException(nameof(predicate)) : this.Count(predicate);
    }

    /// <summary>
    /// 对整个集合执行排序
    /// </summary>
    public void Sort(Comparison<T?>? comparison = null)
    {
        using (BeginBulkOperation())
        {
            var sorted = comparison != null
                ? this.OrderBy(x => x, new ComparisonComparer<T>(comparison)).ToList()
                : this.OrderBy(x => x).ToList();

            Clear();
            foreach (var item in sorted)
            {
                Add(item);
            }
        }
    }

    /// <summary>
    /// 重置整个集合
    /// </summary>
    public void Reset(IEnumerable<T>? newItems)
    {
        using (BeginBulkOperation())
        {
            Clear();
            if (newItems != null)
            {
                AddRange(newItems);
            }
        }
    }

    #endregion

    #region 辅助类

    private class BulkOperationDisposable(ObservableCollectionEx<T> collection) : IDisposable
    {
        public void Dispose()
        {
            collection.EndBulkOperation();
        }
    }

    private class ComparisonComparer<TItem>(Comparison<TItem?>? comparison) : IComparer<TItem>
    {
        public int Compare(TItem? x, TItem? y)
        {
            return comparison?.Invoke(x, y) ?? 0;
        }
    }

    #endregion
}

using System.Reflection;
using System.Text.Json.Serialization;

namespace SnowyRiver.ComponentModel.NotifyPropertyChanged;

using System.Runtime.CompilerServices;

public class NotifyPropertyChangedObjectWithHistory : NotifyPropertyChangedObject
{
    private readonly Dictionary<string, bool> _propertyTrackingCache = new();
    private readonly Stack<PropertyChangeRecord> _undoStack = new();
    private readonly Stack<PropertyChangeRecord> _redoStack = new();
    private bool _isUndoRedoOperation;

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>
    /// Gets the undo history as a read-only collection.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyCollection<PropertyChangeRecord> UndoHistory => _undoStack.ToList().AsReadOnly();

    /// <summary>
    /// Gets the redo history as a read-only collection.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyCollection<PropertyChangeRecord> RedoHistory => _redoStack.ToList().AsReadOnly();

    /// <summary>
    /// Gets the previous value of a specific property from the undo history.
    /// </summary>
    public object? GetPreviousValue(string propertyName)
    {
        var record = _undoStack.FirstOrDefault(r => r.PropertyName == propertyName);
        return record?.Value;
    }

    /// <summary>
    /// Peeks at the most recent undo record without removing it.
    /// </summary>
    public PropertyChangeRecord? PeekUndoRecord()
    {
        return _undoStack.TryPeek(out var record) ? record : null;
    }

    /// <summary>
    /// Peeks at the most recent redo record without removing it.
    /// </summary>
    public PropertyChangeRecord? PeekRedoRecord()
    {
        return _redoStack.TryPeek(out var record) ? record : null;
    }

    /// <summary>
    /// Sets the property value and tracks history. Call this in property setters for proper undo/redo support.
    /// </summary>
    /// <typeparam name="T">Type of the property.</typeparam>
    /// <param name="field">Reference to the property's backing field.</param>
    /// <param name="value">The new value to set.</param>
    /// <param name="propertyName">The property name (automatically provided by CallerMemberName).</param>
    /// <returns>True if the value was changed, false otherwise.</returns>
    protected override bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (base.SetProperty(ref field, value, propertyName))
        {
            if (!_isUndoRedoOperation && propertyName != null && ShouldTrackProperty(propertyName))
            {
                _undoStack.Push(new PropertyChangeRecord(propertyName, field));
                _redoStack.Clear();
                RaisePropertyChanged(() => UndoHistory);
                RaisePropertyChanged(() => CanUndo);
                RaisePropertyChanged(() => RedoHistory);
                RaisePropertyChanged(() => CanRedo);
            }

            return true;
        }

        return false;
    }

     private bool ShouldTrackProperty(string? propertyName)
    {
        if (propertyName == null) return false;

        if (_propertyTrackingCache.TryGetValue(propertyName, out var shouldTrack))
            return shouldTrack;

        var property = GetType().GetProperty(propertyName);
        shouldTrack = property?.GetCustomAttribute<TrackHistoryAttribute>() != null;
        _propertyTrackingCache[propertyName] = shouldTrack;
        return shouldTrack;
    }

    public void Undo()
    {
        if (!CanUndo) return;

        _isUndoRedoOperation = true;
        try
        {
            var record = _undoStack.Pop();
            var property = GetType().GetProperty(record.PropertyName);
            if (property != null && property.CanWrite)
            {
                var currentValue = property.GetValue(this);
                property.SetValue(this, record.Value);
                _redoStack.Push(record with { Value = currentValue });
                RaisePropertyChanged(() => UndoHistory);
                RaisePropertyChanged(() => CanUndo);
                RaisePropertyChanged(() => RedoHistory);
                RaisePropertyChanged(() => CanRedo);
            }
        }
        finally
        {
            _isUndoRedoOperation = false;
        }
    }

    public void Redo()
    {
        if (!CanRedo) return;

        _isUndoRedoOperation = true;
        try
        {
            var record = _redoStack.Pop();
            var property = GetType().GetProperty(record.PropertyName);
            if (property != null && property.CanWrite)
            {
                var currentValue = property.GetValue(this);
                property.SetValue(this, record.Value);
                _undoStack.Push(new PropertyChangeRecord(record.PropertyName, currentValue));
                RaisePropertyChanged(() => UndoHistory);
                RaisePropertyChanged(() => CanUndo);
                RaisePropertyChanged(() => RedoHistory);
                RaisePropertyChanged(() => CanRedo);
            }
        }
        finally
        {
            _isUndoRedoOperation = false;
        }
    }

    public void ClearHistory()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        RaisePropertyChanged(() => UndoHistory);
        RaisePropertyChanged(() => CanUndo);
        RaisePropertyChanged(() => RedoHistory);
        RaisePropertyChanged(() => CanRedo);
    }

    public record PropertyChangeRecord(string PropertyName, object? Value);
}

[AttributeUsage(AttributeTargets.Property)]
public class TrackHistoryAttribute : Attribute { }


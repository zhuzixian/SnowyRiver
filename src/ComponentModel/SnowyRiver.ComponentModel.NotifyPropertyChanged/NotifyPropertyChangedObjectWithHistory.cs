namespace SnowyRiver.ComponentModel.NotifyPropertyChanged;

using System.Runtime.CompilerServices;

public class NotifyPropertyChangedObjectWithHistory : NotifyPropertyChangedObject
{
    private readonly Stack<PropertyChangeRecord> _undoStack = new();
    private readonly Stack<PropertyChangeRecord> _redoStack = new();
    private bool _isUndoRedoOperation;

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>
    /// Gets the undo history as a read-only collection.
    /// </summary>
    public IReadOnlyCollection<PropertyChangeRecord> UndoHistory => _undoStack.ToList().AsReadOnly();

    /// <summary>
    /// Gets the redo history as a read-only collection.
    /// </summary>
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
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        // Record the old value before changing it
        if (!_isUndoRedoOperation && propertyName != null)
        {
            _undoStack.Push(new PropertyChangeRecord(propertyName, field));
            _redoStack.Clear();
            OnPropertyChanged(nameof(UndoHistory));
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(RedoHistory));
            OnPropertyChanged(nameof(CanRedo));
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected override void OnPropertyChanged(string? propertyName)
    {
        if (!_isUndoRedoOperation && propertyName != null)
        {
            var property = GetType().GetProperty(propertyName);
            if (property != null && property is { CanRead: true, CanWrite: true })
            {
                var currentValue = property.GetValue(this);
                _undoStack.Push(new PropertyChangeRecord(propertyName, currentValue));
                _redoStack.Clear();
                OnPropertyChanged(nameof(UndoHistory));
                OnPropertyChanged(nameof(CanUndo));
                OnPropertyChanged(nameof(RedoHistory));
                OnPropertyChanged(nameof(CanRedo));
            }
        }

        base.OnPropertyChanged(propertyName);
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
                _redoStack.Push(new PropertyChangeRecord(record.PropertyName, currentValue));
                OnPropertyChanged(nameof(UndoHistory));
                OnPropertyChanged(nameof(CanUndo));
                OnPropertyChanged(nameof(RedoHistory));
                OnPropertyChanged(nameof(CanRedo));
                OnPropertyChanged(record.PropertyName);
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
                OnPropertyChanged(nameof(UndoHistory));
                OnPropertyChanged(nameof(CanUndo));
                OnPropertyChanged(nameof(RedoHistory));
                OnPropertyChanged(nameof(CanRedo));
                OnPropertyChanged(record.PropertyName);
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
        OnPropertyChanged(nameof(UndoHistory));
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(RedoHistory));
        OnPropertyChanged(nameof(CanRedo));
    }

    public record PropertyChangeRecord(string PropertyName, object? Value);
}

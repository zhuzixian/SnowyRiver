namespace SnowyRiver.ComponentModel.NotifyPropertyChanged;
public class NotifyPropertyChangedObjectWithHistory : NotifyPropertyChangedObject
{
    private readonly Stack<PropertyChangeRecord> _undoStack = new();
    private readonly Stack<PropertyChangeRecord> _redoStack = new();
    private bool _isUndoRedoOperation;

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

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
    }

    private record PropertyChangeRecord(string PropertyName, object? Value);
}

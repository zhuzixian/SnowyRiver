using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FluentValidation;

namespace SnowyRiver.ComponentModel.NotifyPropertyChanged.FluentValidation;

public abstract class ValidatableNotifyPropertyChangedObject<T> : NotifyPropertyChangedObject, INotifyDataErrorInfo
    where T : ValidatableNotifyPropertyChangedObject<T>
{
    protected virtual IValidator<T>? Validator { get; set; }
    private readonly Dictionary<string, List<string>> _errors = new();

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public bool HasErrors => _errors.Any(kv => kv.Value is { Count: > 0 });

    public virtual IEnumerable GetErrors(string? propertyName = null)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return _errors.Values.SelectMany(errors => errors);
        }

        return _errors.TryGetValue(propertyName, out var propertyErrors) 
            ? propertyErrors 
            : Enumerable.Empty<string>();
    }

    protected virtual void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
    }

    protected virtual void SetErrors(string propertyName, IEnumerable<string>? errors)
    {
        var errorsArray = errors == null ? [] : errors as string[] ?? errors.ToArray();
        if (errorsArray.Any())
        {
            _errors[propertyName] = new List<string>(errorsArray);
        }
        else
        {
            _errors.Remove(propertyName);
        }
        OnErrorsChanged(propertyName);
    }

    protected virtual void ClearErrors()
    {
        var propertyNames = _errors.Keys.ToList();
        _errors.Clear();
        foreach (var propertyName in propertyNames)
        {
            OnErrorsChanged(propertyName);
        }
    }

    protected virtual async Task ValidatePropertyAsync([CallerMemberName]string? propertyName = null)
    {
        if(Validator == null) return;

        var validationResult = await Validator.ValidateAsync(this as T,
            options => options.IncludeProperties(propertyName));

        var propertyErrors = validationResult.Errors
            .Where(e => e.PropertyName == propertyName)
            .Select(e => e.ErrorMessage)
            .ToList();
        SetErrors(propertyName, propertyErrors);
    }

    protected virtual void ValidateProperty([CallerMemberName]string? propertyName = null)
    {
        if (Validator == null) return;

        var validationResult = Validator.Validate(this as T,
            options => options.IncludeProperties(propertyName));

        var propertyErrors = validationResult.Errors
            .Where(e => e.PropertyName == propertyName)
            .Select(e => e.ErrorMessage)
            .ToList();
        SetErrors(propertyName, propertyErrors);
    }

    protected virtual async Task<bool> ValidateAsync()
    {
        if(Validator == null) return true;
        var validationResult = await Validator.ValidateAsync(this as T);
        ClearErrors();
        var propertyGroups = validationResult.Errors
            .GroupBy(e => e.PropertyName);
        foreach (var group in propertyGroups)
        {
            var propertyErrors = group.Select(e => e.ErrorMessage).ToList();
            _errors[group.Key] = propertyErrors;
            OnErrorsChanged(group.Key);
        }
        return !HasErrors;
    }

    protected override bool Set<T1>(ref T1 field, T1 value, [CallerMemberName] string propertyName = null)
    {
        var result = base.Set(ref field, value, propertyName);
        if (result)
        {
            ValidateProperty(propertyName);
        }

        return result;
    }

    protected override bool SetProperty<T1>(ref T1 storage, T1 value, [CallerMemberName] string? propertyName = null)
    {
        var result = base.SetProperty(ref storage, value, propertyName);
        if (result)
        {
            ValidateProperty(propertyName);
        }

        return result;
    }

    protected override bool SetProperty<T>(ref T storage, T value, Action? onChanged,
        [CallerMemberName] string? propertyName = null)
    {
        var result = base.SetProperty(ref storage, value, onChanged, propertyName);
        if (result)
        {
            ValidateProperty(propertyName);
        }

        return result;
    }
}
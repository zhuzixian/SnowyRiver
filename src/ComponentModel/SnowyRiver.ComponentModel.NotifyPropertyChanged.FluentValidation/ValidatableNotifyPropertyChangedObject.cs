using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FluentValidation;

namespace SnowyRiver.ComponentModel.NotifyPropertyChanged.FluentValidation;
public class ValidatableNotifyPropertyChangedObject : NotifyPropertyChangedObject, INotifyDataErrorInfo
{
    // 存储属性名及其对应的错误信息列表
    private readonly Dictionary<string, List<string>> _errors = new();

    // 错误变更事件，WPF绑定引擎监听此事件以更新UI
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    // 是否存在任何错误
    public bool HasErrors => _errors.Any(kv => kv.Value.Count > 0);

    // 获取指定属性的错误列表
    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            // 返回所有错误（例如用于表单提交前的整体检查）
            return _errors.Values.SelectMany(errors => errors);
        }

        return _errors.TryGetValue(propertyName, out var errors1) 
            ? errors1 : Enumerable.Empty<string>();
    }

    // 触发错误变更事件
    protected virtual void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors)); // 通知HasErrors属性变化
    }

    // 设置错误
    protected void SetErrors(string propertyName, IEnumerable<string>? errors)
    {
        var errorsArray = errors as string[] ?? errors.ToArray();
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

    // 清除所有错误
    protected void ClearErrors()
    {
        var propertyNames = _errors.Keys.ToList();
        _errors.Clear();
        foreach (var propertyName in propertyNames)
        {
            OnErrorsChanged(propertyName);
        }
    }

    // 核心验证方法：使用FluentValidation验证器进行验证
    protected void ValidateProperty<TValidator>(object value, [CallerMemberName] string propertyName = null)
        where TValidator : AbstractValidator<ValidatableNotifyPropertyChangedObject>, new()
    {
        var validator = new TValidator();
        // 注意：这里简化了，实际可能需要根据属性名进行更精细的验证
        var context = new ValidationContext<ValidatableNotifyPropertyChangedObject>(this)
        {
            RootContextData =
            {
                ["PropertyName"] = propertyName
            }
        };

        var result = validator.Validate(context);

        var propertyErrors = result.Errors
            .Where(e => e.PropertyName == propertyName)
            .Select(e => e.ErrorMessage)
            .ToList();

        SetErrors(propertyName, propertyErrors);
    }

    // 异步验证方法（可选）
    protected virtual async Task ValidatePropertyAsync<TValidator>(object value, [CallerMemberName] string propertyName = null)
        where TValidator : AbstractValidator<ValidatableNotifyPropertyChangedObject>, new()
    {
        await Task.CompletedTask;
        ValidateProperty<TValidator>(value, propertyName);
    }
}

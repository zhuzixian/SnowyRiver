namespace SnowyRiver.WPF.MaterialDesignInPrism.Models;

public class DialogResult<T>(ButtonResult result, T? value) : DialogResult(result)
{
    public DialogResult(): this(ButtonResult.None)
    {
    }

    public DialogResult(ButtonResult result) : this(result, default)
    {
    }

    public T? Value
    {
        get;
        set;
    } = value;
}

using Prism.Mvvm;
using Prism.Navigation;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;
public abstract class ViewModelBase : BindableBase, IDestructible
{
    protected ViewModelBase()
    {

    }

    public virtual void Destroy()
    {

    }
}

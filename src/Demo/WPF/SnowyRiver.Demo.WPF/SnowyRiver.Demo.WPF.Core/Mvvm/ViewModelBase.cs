using Prism.Mvvm;
using Prism.Navigation;

namespace SnowyRiver.Demo.WPF.Core.Mvvm
{
    public abstract class ViewModelBase : BindableBase, IDestructible
    {
        protected ViewModelBase()
        {

        }

        public virtual void Destroy()
        {

        }
    }
}

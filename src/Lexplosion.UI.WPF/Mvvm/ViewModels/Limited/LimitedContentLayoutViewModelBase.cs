using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.ViewModel;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Limited
{
    public abstract class LimitedContentLayoutViewModelBase : ViewModelBase, ILimitedAccessLayout
    {
        public abstract bool HasAccess { get; }
        public virtual ILimitedAccess Content { get; }


        protected LimitedContentLayoutViewModelBase(ILimitedAccess viewModelBase)
        {
            Content = viewModelBase;
        }


        public abstract void RefreshAccessData();
    }
}

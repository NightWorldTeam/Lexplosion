using Lexplosion.WPF.NewInterface.Core.ViewModel;
using Lexplosion.WPF.NewInterface.Core;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Limited
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

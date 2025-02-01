namespace Lexplosion.WPF.NewInterface.Core.ViewModel
{
    public abstract class LimitedContentViewModelBase : ViewModelBase, ILimitedAccess
    {
        public abstract bool HasAccess { get; }

        public abstract void RefreshAccessData();
    }
}

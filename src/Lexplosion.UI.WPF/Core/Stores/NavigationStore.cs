using Lexplosion.UI.WPF.Core;

namespace Lexplosion.UI.WPF.Stores
{
    public sealed class NavigationStore : INavigationStore
    {
        public event CurrentViewModelChangedEventHandler CurrentViewModelChanged;

        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel; set
            {
                _currentViewModel = value;
                OnCurrentViewModelChanged();
            }
        }

        private void OnCurrentViewModelChanged()
        {
            CurrentViewModelChanged?.Invoke();
        }
    }
}

using System;

namespace Lexplosion.Gui.Stores
{
    public sealed class NavigationStore
    {
        public event Action CurrentViewModelChanged;
        private VMBase _currentViewModel;
        private VMBase _prevViewModel;

        // save opened pages here;

        public VMBase CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnCurrentViewModelChanged();
            }
        }

        public VMBase PrevViewModel
        {
            get => _prevViewModel; set
            {
                _prevViewModel = value;
            }
        }

        private void OnCurrentViewModelChanged()
        {
            CurrentViewModelChanged?.Invoke();
        }
    }
}

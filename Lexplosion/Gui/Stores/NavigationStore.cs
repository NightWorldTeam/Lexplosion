using System;

namespace Lexplosion.Gui.Stores
{
    public class NavigationStore
    {
        public event Action CurrentViewModelChanged;
        private VMBase _currentViewModel;

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

        private void OnCurrentViewModelChanged()
        {
            CurrentViewModelChanged?.Invoke();
        }
    }
}

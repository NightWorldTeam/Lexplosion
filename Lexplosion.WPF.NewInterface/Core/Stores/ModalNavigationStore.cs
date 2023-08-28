using Lexplosion.WPF.NewInterface.Core.Modal;

namespace Lexplosion.WPF.NewInterface.Stores
{
    public sealed class ModalNavigationStore
    {
        public static ModalNavigationStore Instance { get; } = new ModalNavigationStore();

        private ModalNavigationStore()
        {

        }


        public event CurrentViewModelChangedEventHandler CurrentViewModelChanged;

        private IModalViewModel _currentViewModel;
        public IModalViewModel CurrentViewModel
        {
            get => _currentViewModel; private set
            {
                _currentViewModel = value;
                OnCurrentViewModelChanged();
            }
        }

        public void Open(IModalViewModel viewModel)
        {
            CurrentViewModel = viewModel;
        }

        public void Close()
        {
            CurrentViewModel = null;
        }

        private void OnCurrentViewModelChanged()
        {
            Runtime.DebugWrite("TESLTLKTSEKLTETKS");
            CurrentViewModelChanged?.Invoke();
        }
    }
}

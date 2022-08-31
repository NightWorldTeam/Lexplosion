using Lexplosion.Gui.ModalWindow;
using Lexplosion.Gui.Stores;

namespace Lexplosion.Gui.ViewModels.ModalVMs
{
    public sealed class ModalWindowViewModel : VMBase
    {
        private readonly NavigationStore ModalWindowNavigationStore = new NavigationStore();

        public ModalVMBase CurrentModalContent => (ModalVMBase)ModalWindowNavigationStore.CurrentViewModel;

        /// <summary>
        /// Данное свойство содержить информации - открыт ли Экспорт [Popup].
        /// </summary>
        private bool _isOpen = false;
        public bool IsOpen
        {
            get => _isOpen; set
            {
                _isOpen = value;
                OnPropertyChanged();
            }
        }

        public ModalWindowViewModel()
        {
            ModalWindowNavigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }


        public void ChangeCurrentModalContent(ModalVMBase modalVM)
        {
            ModalWindowNavigationStore.CurrentViewModel = modalVM;
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentModalContent));
        }

        public void OpenWindow(ModalVMBase modalVM)
        {
            IsOpen = true;
            ChangeCurrentModalContent(modalVM);
        }

        public void CloseWindow()
        {
            IsOpen = false;
            ChangeCurrentModalContent(null);
        }
    }
}

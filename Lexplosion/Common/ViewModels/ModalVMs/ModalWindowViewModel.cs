using Lexplosion.Common.ModalWindow;
using Lexplosion.Common.Stores;

namespace Lexplosion.Common.ViewModels.ModalVMs
{
    public sealed class ModalWindowViewModelSingleton : VMBase
    {
        public static ModalWindowViewModelSingleton Instance { get; } = new ModalWindowViewModelSingleton();


        private readonly NavigationStore ModalWindowNavigationStore = new NavigationStore();


        #region Properties


        public ModalVMBase CurrentModalContent => (ModalVMBase)ModalWindowNavigationStore.CurrentViewModel;

        /// <summary>
        /// Данное свойство содержить информации - открыт ли Экспорт [Popup].
        /// </summary>
        private bool _isOpen = false;
        public bool IsOpen
        {
            get => _isOpen; private set
            {
                _isOpen = value;
                OnPropertyChanged();
            }
        }

        public bool IsHidden { get; private set; }


        #endregion Properties


        #region Constructors


        private ModalWindowViewModelSingleton()
        {
            ModalWindowNavigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }


        #endregion Constructors


        #region Public & Protected Methods


        public void Open(ModalVMBase modalVM)
        {
            IsOpen = true;
            ChangeCurrentModalContent(modalVM);
        }

        public void Close()
        {
            IsOpen = false;
            ChangeCurrentModalContent(null);
        }

        public void Expand()
        {
            IsOpen = true;
            IsHidden = !IsOpen;
        }

        public void Hide()
        {
            IsOpen = false;
            IsHidden = !IsOpen;
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void ChangeCurrentModalContent(ModalVMBase modalVM)
        {
            ModalWindowNavigationStore.CurrentViewModel = modalVM;
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentModalContent));
        }


        #endregion Private Methods
    }
}

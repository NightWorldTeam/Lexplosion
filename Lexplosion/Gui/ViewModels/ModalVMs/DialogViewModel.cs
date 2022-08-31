using Lexplosion.Gui.ModalWindow;
using System;

namespace Lexplosion.Gui.ViewModels.ModalVMs
{
    public sealed class DialogViewModel : ModalVMBase
    {
        private MainViewModel _mainViewModel;
        private Action _function;
        private string _title;

        private const double _width = 300;
        private const double _height = 250;

        #region commands

        private RelayCommand _actionCommand;
        public override RelayCommand Action {
            get => _actionCommand ?? new RelayCommand(obj => 
            {
                _function();
                _mainViewModel.ModalWindowVM.IsOpen = false;
            });
        }

        private RelayCommand _cancelButtonCommand;
        public override RelayCommand CloseModalWindow
        {
            get => _cancelButtonCommand ?? new RelayCommand(obj => 
            {
                _mainViewModel.ModalWindowVM.IsOpen = false;
            });
        }

        #endregion

        public override double Width => _width;
        public override double Height => _height;

        public string Title 
        {
            get => _title; set 
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public DialogViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        /// <summary>
        /// Показывает диалоговое окно.
        /// </summary>
        /// <param name="title">Title - заголовк для dialog window</param>
        /// <param name="function">Делегат который выполниться при нажатии пользователем кнопки "Да".</param>
        public void ShowDialog(string title, Action function) 
        {
            _mainViewModel.ModalWindowVM.ChangeCurrentModalContent(this);
            _mainViewModel.ModalWindowVM.IsOpen = true;

            _title = title;
            _function = function;
        }
    }
}

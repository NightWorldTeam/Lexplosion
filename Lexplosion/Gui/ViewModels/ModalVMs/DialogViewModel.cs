using Lexplosion.Gui.ModalWindow;
using System;

namespace Lexplosion.Gui.ViewModels.ModalVMs
{
    public sealed class DialogViewModel : ModalVMBase
    {
        private readonly MainViewModel _mainViewModel;
        private Action _function;
        private string _title;

        private const double _width = 300;
        private const double _height = 250;


        #region Properties


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

        #endregion Properties


        #region Commands

        private RelayCommand _actionCommand;
        public override RelayCommand ActionCommand
        {
            get => _actionCommand ?? (_actionCommand = new RelayCommand(obj =>
            {
                _function();
                _mainViewModel.ModalWindowVM.IsOpen = false;
            }));
        }

        private RelayCommand _closeButtonCommand;
        public override RelayCommand CloseModalWindowCommand
        {
            get => _closeButtonCommand ?? (_closeButtonCommand = new RelayCommand(obj =>
            {
                _mainViewModel.ModalWindowVM.IsOpen = false;
            }));
        }

        #endregion Commands


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

            Title = title;
            _function = function;
        }
    }
}

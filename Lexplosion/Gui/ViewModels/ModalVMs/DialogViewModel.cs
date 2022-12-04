using Lexplosion.Gui.ModalWindow;
using System;

namespace Lexplosion.Gui.ViewModels.ModalVMs
{
    public sealed class DialogViewModel : ModalVMBase
    {
        private readonly MainViewModel _mainViewModel;
        private Action _function;
        private string _title;
        private string _message;

        private readonly double _width = 300;
        private readonly double _height = 200;


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

        public string Message 
        {
            get => _message; set 
            {
                _message = value;
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


        public DialogViewModel(MainViewModel mainViewModel, double modalWidth = 300, double modalHeight = 200)
        {
            _mainViewModel = mainViewModel;
            _width = modalWidth;
            _height = modalHeight;
        }

        /// <summary>
        /// Показывает диалоговое окно.
        /// </summary>
        /// <param name="title">Title - заголовк для dialog window</param>
        /// <param name="title">Message - основной текст под заголовком для dialog window</param>
        /// <param name="function">Делегат который выполниться при нажатии пользователем кнопки "Да".</param>
        public void ShowDialog(string title, string message, Action function)
        {
            _mainViewModel.ModalWindowVM.ChangeCurrentModalContent(this);
            _mainViewModel.ModalWindowVM.IsOpen = true;

            Title = title;
            Message = message;
            _function = function;
        }
    }
}

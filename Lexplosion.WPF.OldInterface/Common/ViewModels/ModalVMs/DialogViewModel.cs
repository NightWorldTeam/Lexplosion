using Lexplosion.Common.ModalWindow;
using System;

namespace Lexplosion.Common.ViewModels.ModalVMs
{
    public sealed class DialogViewModel : ModalVMBase
    {
        private Action _function;
        private Action _function1;
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
                ModalWindowViewModelSingleton.Instance.Close();
            }));
        }

        private RelayCommand _closeButtonCommand;
        public override RelayCommand CloseModalWindowCommand
        {
            get => _closeButtonCommand ?? (_closeButtonCommand = new RelayCommand(obj =>
            {
                _function1?.Invoke();
                ModalWindowViewModelSingleton.Instance.Close();
            }));
        }

        #endregion Commands


        public DialogViewModel(double modalWidth = 300, double modalHeight = 200)
        {
            _width = modalWidth;
            _height = modalHeight;
        }

        /// <summary>
        /// Показывает диалоговое окно.
        /// </summary>
        /// <param name="title">Title - заголовк для dialog window</param>
        /// <param name="title">Message - основной текст под заголовком для dialog window</param>
        /// <param name="function">Делегат который выполниться при нажатии пользователем кнопки "Да".</param>
        /// <param name="function">Делегат который выполниться при нажатии пользователем кнопки "Нет".</param>
        public void ShowDialog(string title, string message, Action function, Action function1 = null)
        {
            ModalWindowViewModelSingleton.Instance.Open(this);
            Title = title;
            Message = message;
            _function = function;
            _function1 = function1;
        }
    }
}

using Lexplosion.Tools;
using System;

namespace Lexplosion.Gui.ViewModels
{
    /// <summary>
    /// Создано для того, чтобы в любой точке кода которая 
    /// разрабатывается можно было вывести, что блок разрабатываться
    /// </summary>
    public class DevСurtainViewModel : VMBase
    {
        private string _defaultMessage;
        private string _defaultMessageWithName = ResourceGetter.GetString("devCurtainsMessage1");
        private Action _buttonAction;

        private string _message;
        public string Message
        {
            get => _message; set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        private string _buttonContent;
        public string ButtonContent 
        {
            get => _buttonContent; set 
            {
                _buttonContent = value; 
                OnPropertyChanged();
            }
        }

        private RelayCommand _buttonActionCommand;
        public RelayCommand ButtonActionCommand
        {
            get => _buttonActionCommand ?? (_buttonActionCommand = new RelayCommand(obj => 
            {
                _buttonAction();
            }));
        }

        private bool _isButtonEnable = false;
        public bool IsButtonEnable 
        {
            get => _isButtonEnable; set 
            {
                _isButtonEnable = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Будет использоваться стандартное сообщение.
        /// </summary>
        public DevСurtainViewModel(string buttonContent = "", Action buttonAction = null)
        {
            if (buttonContent?.Length != 0 && buttonAction != null) 
            {
                IsButtonEnable = true;
            }

            _defaultMessage = ResourceGetter.GetString("devCurtainMessage");
            Message = _defaultMessage;
        }
    }
}

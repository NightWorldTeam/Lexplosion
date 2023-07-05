using System;

namespace Lexplosion.WPF.NewInterface.ViewModels.Authorization
{
    public class DigitCodeConfirmationVIewModel : VMBase
    {
        private bool _isFullCode;
        public bool IsFullCode 
        {
            protected get => _isFullCode; set 
            {
                _isFullCode = value;
                OnPropertyChanged();
            }
        }

        private string _code;
        public string Code 
        {
            get => _code; set 
            {
                _code = value;
                OnPropertyChanged();
            }
        }

        public DigitCodeConfirmationVIewModel()
        {

        }
    }
}

using System;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Authorization.BasicAuthorization.NightWorld
{
    public class DigitCodeConfimationModel : VMBase
    {
        private string _code;
        public string Code
        {
            get => _code; set
            {
                _code = value;
                OnPropertyChanged();
            }
        }

        private bool _isFullCode;
        public bool IsFullCode
        {
            get => _isFullCode; set
            {
                _isFullCode = value;
                OnPropertyChanged();
            }
        }

        private string _codeEmail;
        public string CodeEmail
        {
            get => _codeEmail; set
            {
                _codeEmail = value;
                OnPropertyChanged();
            }
        }

        public void Check()
        {
            if (!string.IsNullOrEmpty(Code))
            {
            }
            else
            {
                new Exception("is null");
            }
        }
    }
}

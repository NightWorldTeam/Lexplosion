using Lexplosion.UI.WPF.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Lexplosion.UI.WPF.Mvvm.Models.Authorization
{
    public sealed class NightWorldRegistrationModel : ViewModelBase
    {
        private readonly AppCore _appCore;


        #region Properties


        private string _login;
        public string Login
        {
            get => _login; set
            {
                _login = value;
                OnPropertyChanged();
            }
        }

        private string _emain;
        public string Email 
        {
            get => _emain; set 
            {
                _emain = value;
                OnPropertyChanged();
            }
        }

        private string _password;
        public string Password
        {
            get => _password; set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        private string _repeatPassword;
        public string RepeatPassword
        {
            get => _repeatPassword; set
            {
                _repeatPassword = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        public NightWorldRegistrationModel(AppCore appCore)
        {
            _appCore = appCore;
        }


        #region Public Methods


        public void Register() 
        {
            if (!ValidForm()) 
            {
                return;
            }

            // registration here
        }


        #endregion Public Methods


        #region Private Methods


        bool ValidForm() 
        {
            if (string.IsNullOrWhiteSpace(Login))
            {
                _appCore.MessageService.Error("Поле логин должно быть заполнено");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                _appCore.MessageService.Error("Поле \"Почта\" должно быть заполнено");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                _appCore.MessageService.Error("Поле \"Пароль\" должно быть заполнено");
                return false;
            }

            if (string.IsNullOrWhiteSpace(RepeatPassword))
            {
                _appCore.MessageService.Error("Поле \"Повторение Пароля\" должно быть заполнено");
                return false;
            }

            if (Login.Length < 4)
            {
                _appCore.MessageService.Error("Логин не должен быть меньше 4 символов");
                return false;
            }

            if (!IsValidEmail(Email))
            {
                _appCore.MessageService.Error("Электронная почта имеет неверный формат.");
                return false;
            }

            if (Password.Length < 6) 
            {
                _appCore.MessageService.Error("Минимальная длинна пароля 6 символов.");
                return false;
            }

            if (Password == RepeatPassword) 
            {
                _appCore.MessageService.Info("Пароли не совпадают");
                return false;
            }

            return true;
        }


        bool IsValidEmail(string email) 
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false; // suggested by @TK-421
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }


        #endregion Private Methods
    }
}

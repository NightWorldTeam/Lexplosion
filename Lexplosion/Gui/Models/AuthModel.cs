using Lexplosion.Global;
using Lexplosion.Gui.ViewModels;
using Lexplosion.Logic.Management.Authentication;
using Lexplosion.Logic.Network;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lexplosion.Gui.Models
{
    public class AuthModel : VMBase
    {
        private AccountType _accountType;
        private bool _isSavedAccountOAuth2 = false;
        private readonly Authentication _authentication;
        private readonly MainViewModel _mainViewModel;
        private readonly ICommand _navigationCommand;

        public AuthModel(MainViewModel mainViewModel, ref ICommand navigationCommand)
        {
            _mainViewModel = mainViewModel;
            _navigationCommand = navigationCommand;

            // получаем последний выбранный аккаунт
            _authentication = new Authentication();
            _accountTypeSelectedIndex = (int)LoadSavedAccount(null);

            CommandReceiver.MicrosoftAuthPassed += PreformAuthMicrosoft;
        }

        #region Properties

        public bool NoAccountAuth { get => _accountType == AccountType.NoAuth; }

        private bool _isMicrosoftAccountManager = false;
        public bool IsMicrosoftAccountManager
        {
            get => _isMicrosoftAccountManager; set
            {
                _isMicrosoftAccountManager = value;
                OnPropertyChanged();
            }
        }

        private bool _isAccountSaved;
        public bool IsAccountSaved
        {
            get => _isAccountSaved; set
            {
                _isAccountSaved = value;
                OnPropertyChanged();
            }
        }

        #region Main Auth

        private string _savedLogin = string.Empty;
        private string _login = String.Empty;
        /// <summary>
        /// Содержит логин пользователя.
        /// </summary>
        public string Login
        {
            get => _login; set
            {
                _login = value;
                OnPropertyChanged();
            }
        }

        private string _password = String.Empty;
        /// <summary>
        /// Cодержит пароль пользователя.
        /// </summary>
        public string Password
        {
            get => _password; set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        private bool _isSaveMe = false;
        /// <summary>
        /// Хранит ответ на вопрос, хочет ли пользователь сохранить аккаунт(данные).
        /// </summary>
        public bool IsSaveMe
        {
            get => _isSaveMe; set
            {
                _isSaveMe = value;
                OnPropertyChanged();
            }
        }

        private int _accountTypeSelectedIndex = 1;
        /// <summary>
        /// Хранит индекс выбранного типа авторизации.
        /// [0] - NoAuth
        /// [1] - NightWorld
        /// [2] - Mojang
        /// </summary>
        public int AccountTypeSelectedIndex
        {
            get => _accountTypeSelectedIndex; set
            {
                _accountTypeSelectedIndex = value;
                OnPropertyChanged();
                if (_accountTypeSelectedIndex == 3)
                {
                    LoadSavedAccount(AccountType.Microsoft);
                    if (_isSavedAccountOAuth2)
                        IsMicrosoftAccountManager = true;
                    else FollowToMicrosoft();
                }
                LoadSavedAccount((AccountType)_accountTypeSelectedIndex);
                OnPropertyChanged(nameof(NoAccountAuth));
            }
        }

        #endregion Main Auth

        private bool _isAuthing = false;
        /// <summary>
        /// Запустил ли пользователь метод авторизации.
        /// </summary>
        public bool IsAuthProcessStarted
        {
            get => _isAuthing; set
            {
                _isAuthing = value;
                OnPropertyChanged();
            }
        }

        private bool _isAuthFinished = true;
        public bool IsAuthFinished
        {
            get => _isAuthFinished; set
            {
                _isAuthFinished = value;
                OnPropertyChanged();
            }
        }

        private string _manualInputedMicrosoftData;
        public string ManualInputedMicrosoftData 
        {
            get => _manualInputedMicrosoftData; set 
            {
                _manualInputedMicrosoftData = value;
                OnPropertyChanged();
            }
        }

        #endregion Properties


        #region Methods

        public void SignIn() 
        {
            if (!IsAuthProcessStarted)
            {
                IsAuthProcessStarted = false;
                if (_accountType == AccountType.NoAuth && !string.IsNullOrEmpty(Login))
                {
                    Authorization();
                }
                else if (!string.IsNullOrEmpty(Login) && (IsAccountSaved || !string.IsNullOrEmpty(Password)))
                {
                    Authorization();
                }
                else
                {
                    MainViewModel.ShowToastMessage("Заполните логин и пароль!", "Алло! А кто будет данными заполять? :)", Controls.ToastMessageState.Error);
                }
            }
        }

        /// <summary>
        /// Загружает данные сохранённого аккаунта.
        /// </summary>
        /// <param name="accountType">Тип аккаунта, если null, то возвращает последний использованный сохранённый аккаунт.</param>
        public AccountType LoadSavedAccount(AccountType? accountType)
        {
            AccountType type = _authentication.GetAccount(accountType, out _savedLogin);

            if (!string.IsNullOrEmpty(_savedLogin))
            {
                IsAccountSaved = true;
                // так как логин сохранён, а при авторизации Microsoft
                // логин == Ник, мы выводим авторизацию с сохранёным аккаунтов для Microsoft
                if (type == AccountType.Microsoft)
                {
                    _isSavedAccountOAuth2 = true;
                    IsMicrosoftAccountManager = true;
                }
                else IsMicrosoftAccountManager = false;

                IsSaveMe = true;
                Login = _savedLogin; Password = "";
            }
            else
            {
                IsAccountSaved = false;
                if (type == AccountType.Microsoft)
                {
                    _isSavedAccountOAuth2 = false;
                    IsMicrosoftAccountManager = false;
                }
                else IsMicrosoftAccountManager = false;

                Login = ""; Password = "";
            }

            _accountType = type;

            return type;
        }


        /// <summary>
        /// Запускает процесс авторизации.
        /// </summary>
        private void Authorization()
        {
            Lexplosion.Runtime.TaskRun(() =>
            {
                // получаем выбранный тип акканута.
                _accountType = (AccountType)AccountTypeSelectedIndex;

                // получаем ответ от проверки данных.
                AuthCode authCode = _authentication.Auth(_accountType, (Login == _savedLogin) ? null : Login, Password == "" ? null : Password, IsSaveMe);

                App.Current.Dispatcher.Invoke(() =>
                {
                    PerformAuthCode(authCode);
                });

                IsAuthProcessStarted = false;
            });
        }


        private void PreformAuthMicrosoft(string microsoftData, MicrosoftAuthRes result)
        {
            // на случае нештатной ситуации.
            if (_accountType != AccountType.Microsoft)
                return;
            Console.WriteLine(microsoftData);
            var authCode = _authentication.Auth(_accountType, "", microsoftData, true);
            PerformAuthCode(authCode);
            NativeMethods.ShowProcessWindows(Runtime.CurrentProcess.MainWindowHandle);
        }

        private void PerformAuthCode(AuthCode authCode)
        {
            // обрабатываем полученный код.
            switch (authCode)
            {
                case AuthCode.Successfully:
                    {
                        CommandReceiver.MicrosoftAuthPassed -= PreformAuthMicrosoft;

                        _mainViewModel.UserProfile.Nickname = GlobalData.User.Login;
                        _mainViewModel.UserProfile.IsAuthorized = true;
                        _mainViewModel.UserProfile.IsNightWorldAccount = _accountType == AccountType.NightWorld;

                        _navigationCommand.Execute(null);

                        _mainViewModel.SubscribeToOpenModpackEvent();

                        IsAuthFinished = true;
                        break;
                    }
                case AuthCode.DataError:
                    {
                        MainViewModel.ShowToastMessage("Ошибка авторизации", "Неверный логин или пароль.", TimeSpan.FromSeconds(8), Controls.ToastMessageState.Error);
                        IsAuthFinished = true;
                        break;
                    }
                case AuthCode.NoConnect:
                    {
                        MainViewModel.ShowToastMessage("Ошибка авторизации", "Нет соединения с сервером.", TimeSpan.FromSeconds(8), Controls.ToastMessageState.Error);
                        IsAuthFinished = true;
                        break;
                    }
                case AuthCode.TokenError:
                    {
                        MainViewModel.ShowToastMessage("Ошибка авторизации", "Ошибка с токеном.", TimeSpan.FromSeconds(8), Controls.ToastMessageState.Error);
                        FollowToMicrosoft();
                        IsAuthFinished = true;
                        break;
                    }
                case AuthCode.SessionExpired:
                    {
                        MainViewModel.ShowToastMessage("Ошибка входа", "Сессия истекла. Стоит попробовать снова ввести пароль.", TimeSpan.FromSeconds(8), Controls.ToastMessageState.Error);
                        IsAuthFinished = true;
                        break;
                    }
                default:
                    {
                        MainViewModel.ShowToastMessage("Ошибка. Что-то не так", authCode.ToString(), TimeSpan.FromSeconds(8), Controls.ToastMessageState.Error);
                        IsAuthFinished = true;
                        break;
                    }
            }
        }


        public void FollowToMicrosoft()
        {
            IsAuthFinished = false;
            System.Diagnostics.Process.Start("https://login.live.com/oauth20_authorize.srf?client_id=ed0f84c7-4bf4-4a97-96c7-8c82b1e4ea0b&response_type=code&redirect_uri=https://night-world.org/requestProcessing/microsoftOAuth.php&scope=XboxLive.signin%20offline_access&state=NOT_NEEDED");
        }

        public void CancelMicrosoftAuth()
        {
            AccountTypeSelectedIndex = 1;
            IsAuthFinished = true;
        }

        public void MicrosoftManualInput() 
        {
            var authCode = _authentication.Auth(_accountType, "", ManualInputedMicrosoftData, true);
            PerformAuthCode(authCode);
            NativeMethods.ShowProcessWindows(Runtime.CurrentProcess.MainWindowHandle);
        }
        #endregion Methods
    }
}

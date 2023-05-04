using Lexplosion.Controls;
using Lexplosion.Global;
using Lexplosion.Logic.Management.Authentication;
using Lexplosion.Logic.Network;
using Lexplosion.Tools;
using System;

namespace Lexplosion.Common.Models
{
    public sealed class AuthModel : VMBase
    {
        private AccountType _accountType;
        private bool _isSavedAccountOAuth2 = false;
        private readonly Authentication _authentication;

        private readonly Action<string, bool, bool> _successfulAuthorization;
        private readonly DoNotificationCallback _doNotification = (header, message, time, type) => { };

        /// <summary>
        /// Constructors for AuthModel with Messages;
        /// </summary>
        /// <param name="successfulAuthorization">Action<Nickname, IsAuth, IsNightWorldAccount></param>
        /// <param name="doNotification">Action<Header, Message, Time, Type></param>
        public AuthModel(Action<string, bool, bool> successfulAuthorization, DoNotificationCallback doNotification)
        {
            _doNotification = doNotification;
            _successfulAuthorization = successfulAuthorization;

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


        #region Public & Protected Methods


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
                    _doNotification(ResourceGetter.GetString("fillLoginAndPassword"), ResourceGetter.GetString("heyWhoWillFiilField"), 8, 1);
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

        #endregion Public & Protected Methods


        #region Private Methods


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
                AuthCode authCode = _authentication.Auth(_accountType, (Login == _savedLogin) ? null : Login, Password?.Length == 0 ? null : Password, IsSaveMe);

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

                        _successfulAuthorization(GlobalData.User.Login, true, _accountType == AccountType.NightWorld);

                        IsAuthFinished = true;
                        break;
                    }
                case AuthCode.DataError:
                    {
                        _doNotification(ResourceGetter.GetString("authorisationError"), ResourceGetter.GetString("wrongLoginOrPassword"), 8, 0);
                        IsAuthFinished = true;
                        break;
                    }
                case AuthCode.NoConnect:
                    {
                        _doNotification(ResourceGetter.GetString("authorisationError"), ResourceGetter.GetString("noConnetionsToTheServer"), 8, 0);
                        IsAuthFinished = true;
                        break;
                    }
                case AuthCode.TokenError:
                    {
                        _doNotification(ResourceGetter.GetString("authorisationError"), ResourceGetter.GetString("tokenError"), 8, 0);
                        FollowToMicrosoft();
                        IsAuthFinished = true;
                        break;
                    }
                case AuthCode.SessionExpired:
                    {
                        _doNotification(ResourceGetter.GetString("loginFailed"), ResourceGetter.GetString("sessionExpiredPleaseTryAgainFillPassword"), 8, 0);
                        IsAuthFinished = true;
                        break;
                    }
                default:
                    {
                        _doNotification(ResourceGetter.GetString("someError"), authCode.ToString(), 8, 0);
                        IsAuthFinished = true;
                        break;
                    }
            }
        }


        #endregion Private Methods
    }
}

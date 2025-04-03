﻿using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Network;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Authorization;
using System;
using System.Windows.Forms;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Modal
{
    public class AccountFactoryModel : ViewModelBase
    {
        private readonly Action AuthFinished;
        private readonly AppCore _appCore;
        private readonly Action _toAccountFactory;


        private event Action<string> MicrosoftInputTokenPassed;

        private Action UnsubscribeMicrosoftAuthPassedEvent;

        #region Properties


        private AccountType _accountType = AccountType.NightWorld;
        public AccountType AccountType
        {
            get => _accountType; set
            {
                _accountType = value;
                CheckAllFieldCorrect();
                OnPropertyChanged();
            }
        }

        private string _login = string.Empty;
        public string Login
        {
            get => _login; set
            {
                _login = value;

                CheckAllFieldCorrect();
                OnPropertyChanged();
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password; set
            {
                _password = value;
                CheckAllFieldCorrect();
                OnPropertyChanged();
            }
        }

        private bool _isAuthorizationInProgress;
        public bool IsAuthorizationInProcess
        {
            get => _isAuthorizationInProgress; private set
            {
                _isAuthorizationInProgress = value;
                OnPropertyChanged();
            }
        }


        public bool IsAllFieldCorrect { get; private set; } = false;


        #endregion Properties


        public AccountFactoryModel(AppCore appCore, Action authFinished, Action toAccountFactory)
        {
            _appCore = appCore;
            AuthFinished = authFinished;
            _toAccountFactory = toAccountFactory;
        }


        // NoAuth       |$>> Login
        // NightWorld   |$>> Login + Password
        // Microsoft    |$>> Login + redirect to Microsoft

        public void Auth()
        {
            IsAuthorizationInProcess = true;
            switch (AccountType)
            {
                case AccountType.NoAuth:
                    {
                        NoAuth();
                        break;
                    }
                case AccountType.NightWorld:
                    {
                        NightWorldAuth();
                        break;
                    }

                case AccountType.Microsoft:
                    {
                        MicrosoftAuth();
                        break;
                    }
                default:
                    IsAuthorizationInProcess = false;
                    break;
            }
            ;
        }

        /// <summary>
        /// Отмена авторизации.
        /// </summary>
        public void Cancel()
        {
            AuthFinished?.Invoke();
            UnsubscribeMicrosoftAuthPassedEvent?.Invoke();
        }

        /// <summary>
        /// Запускает модальное окно ручного ввода.
        /// </summary>
        public void ManualInput()
        {
            var manualInput = new MicrosoftManualInputViewModel(_appCore);

            /// Обрабатываем ввод токена
            manualInput.TokenEntered += (token) =>
            {
                _toAccountFactory?.Invoke();
                IsAuthorizationInProcess = true;
                MicrosoftInputTokenPassed?.Invoke(token);
            };

            /// Открываем модальное окно ввода токена
            _appCore.ModalNavigationStore.Open(manualInput);

            /// Обработка изменения состояния модального она, 
            /// если оно закрывается, то открываем окно accountfactory;
            _appCore.ModalNavigationStore.CurrentViewModelChanged += OpenAccountFactoryModal;
        }

        void OpenAccountFactoryModal() 
        {
            _appCore.ModalNavigationStore.CurrentViewModelChanged -= OpenAccountFactoryModal;
            _toAccountFactory?.Invoke();
            IsAuthorizationInProcess = true;
        }

        private void NoAuth()
        {
            var account = new Account(AccountType.NoAuth, Login);
            account.Save();
            IsAuthorizationInProcess = true;
            AuthFinished?.Invoke();
        }

        private void NightWorldAuth()
        {
            var account = new Account(AccountType.NightWorld, Login);

            Runtime.TaskRun(() =>
            {
                var authCode = account.Auth(Password);
                if (authCode == AuthCode.Successfully)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (Account.ActiveAccount == null)
                        {
                            account.IsActive = true;
                        }

                        account.Save();
                        IsAuthorizationInProcess = false;
                        AuthFinished?.Invoke();
                    });
                }
            });
        }

        private void MicrosoftAuth()
        {
            var account = new Account(AccountType.Microsoft);

            void SuccessAuth(string token, MicrosoftAuthRes res)
            {
                if (res == MicrosoftAuthRes.Successful)
                {
                    Runtime.TaskRun(() =>
                    {
                        var code = account.Auth(token);
                        if (code == AuthCode.Successfully)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                account.Save();
                                CommandReceiver.MicrosoftAuthPassed -= SuccessAuth;
                                IsAuthorizationInProcess = false;
                                AuthFinished?.Invoke();
                            });
                        }

                        _appCore.ModalNavigationStore.Close();
                    });
                }
            }

            CommandReceiver.MicrosoftAuthPassed += SuccessAuth;

            UnsubscribeMicrosoftAuthPassedEvent = () => 
            {
                CommandReceiver.MicrosoftAuthPassed -= SuccessAuth;
                UnsubscribeMicrosoftAuthPassedEvent = null;
            };

            MicrosoftInputTokenPassed += (token) => 
            {
                SuccessAuth(token, MicrosoftAuthRes.Successful);
            };
            // TODO: Засунуть это в констану.
            System.Diagnostics.Process.Start("https://login.live.com/oauth20_authorize.srf?client_id=ed0f84c7-4bf4-4a97-96c7-8c82b1e4ea0b&response_type=code&redirect_uri=https://night-world.org/requestProcessing/microsoftOAuth.php&scope=XboxLive.signin%20offline_access&state=NOT_NEEDED");
        }



        private bool CheckAllFieldCorrect()
        {
            var loginFlag = true;
            var passwordFlag = true;

            if (AccountType == AccountType.Microsoft)
            {
                IsAllFieldCorrect = true;
                OnPropertyChanged(nameof(IsAllFieldCorrect));
                return true;
            }

            loginFlag = IsLoginCorrect(Login);

            if (AccountType != AccountType.NoAuth)
            {
                passwordFlag = IsPasswordCorrect(Password);
            }

            IsAllFieldCorrect = loginFlag && passwordFlag;
            OnPropertyChanged(nameof(IsAllFieldCorrect));

            return IsAllFieldCorrect;
        }



        private bool IsLoginCorrect(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            if (value.Length < 3 || value.Length > 16)
                return false;

            return true;
        }

        private bool IsPasswordCorrect(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            return true;
        }
    }
}

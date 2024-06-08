using Lexplosion.Global;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Management.Accounts.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lexplosion.Logic.FileSystem;
using Newtonsoft.Json;

namespace Lexplosion.Logic.Management.Accounts
{
    public class Account : VMBase
    {
        private class AccountSummary
        {
            public AccountType AccountType;
            public string Login;
            public string UUID;
            public string AccessData;
            public bool IsActive;
            public bool IsLaunched;

            public bool IsValid()
            {
                return !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(UUID) && Enum.IsDefined(typeof(AccountType), (int)AccountType);
            }
        }

        public string Login { get; private set; }
        public string UUID { get; private set; }
        public string AccessToken { get; private set; }
        public string SessionToken { get; private set; }
        public AccountType AccountType { get; private set; }

        private ActivityStatus _status = ActivityStatus.Offline;
        public ActivityStatus Status
        {
            get => (IsActive || IsLaunched) ? _status : ActivityStatus.Offline;
            private set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Является ли аккаунт активным. 
        /// Если установить true, то IsActive другого аккаунта, который был запускаемым изменится на false.
        /// При установки true, статичное свойство ActiveAccount примет значение в виде этого аккаунта.
        /// Значение true могут иметь только аккаунты NightWorld, если попытаться установить IsActive = true другому типу аккаунта, то ничего не произойдет.
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                if (_isActive)
                {
                    if (AccountType == AccountType.NightWorld)
                    {
                        if (ActiveAccount != null) ActiveAccount.IsActive = false;
                        ActiveAccount = this;
                        TryInitNwServices();
                    }
                }
                else
                {
                    ActiveAccount = null;
                    TryStopNwServices();
                }
            }
        }
        private bool _isActive = false;

        /// <summary>
        /// Является ли аккаунт запускаемым. 
        /// Если установить true, то IsLaunched другого аккаунта, который был запускаемым изменится на false.
        /// При установке true, статичное свойство LaunchedAccount примет значение в виде этого аккаунта.
        /// Так же, если установить true аккаунту NightWorld, то IsActive у этого аккаунта тоже примет значение true.
        /// </summary>
        public bool IsLaunched
        {
            get => _isLaunched;
            set
            {
                _isLaunched = value;
                if (_isLaunched)
                {
                    if (LaunchedAccount != null) LaunchedAccount.IsLaunched = false;
                    LaunchedAccount = this;
                    if (AccountType == AccountType.NightWorld)
                    {
                        IsActive = true;
                    }
                }
                else
                {
                    LaunchedAccount = null;
                }
            }
        }
        private bool _isLaunched = false;

        private string _accessData;

        private static Dictionary<Account, AccountSummary> _listToSave;
        private static HashSet<Account> _accounts;

        public bool IsAuthed { get; private set; } = false;

        /// <summary>
        /// Список сохраненных акаунтов
        /// </summary>
        public static IEnumerable<Account> List { get => _accounts; }
        /// <summary>
        /// Количество сохраненных аккаунтов
        /// </summary>
        public static int ListCount { get => _accounts.Count; }
        /// <summary>
        /// Аккаунт, который активен
        /// </summary>
        public static Account ActiveAccount { get; private set; }

        /// <summary>
        /// Возвращает запускаемый аккаунт. Если он не установлен, то вернется значение свойство ActiveAccount
        /// </summary>
        public static Account LaunchedAccount
        {
            get => _launchedAccount ?? ActiveAccount;
            private set => _launchedAccount = value;
        }
        private static Account _launchedAccount = null;

        static Account()
        {
            // TODO: не забыть сделать совместимость со старым файлом аккаунтов  
            var list = DataFilesManager.GetFile<HashSet<AccountSummary>>(LaunсherSettings.LauncherDataPath + "/accounts.json");
            _listToSave = new Dictionary<Account, AccountSummary>();
            _accounts = new HashSet<Account>();

            if (list != null)
            {
                foreach (var item in list)
                {
                    if (item.IsValid())
                    {
                        var account = new Account(item);
                        AddToList(account);
                    }
                }
            }
        }

        private static void AddToList(Account account)
        {
            _accounts.Add(account);
            _listToSave[account] = account.GetAccountSummary();
        }

        private static void SaveSummaryList()
        {
            DataFilesManager.SaveFile(LaunсherSettings.LauncherDataPath + "/accounts.json", JsonConvert.SerializeObject(_listToSave.Values));
        }

        public Account(AccountType type)
        {
            AccountType = type;
        }

        public Account(AccountType type, string login) : this(type)
        {
            Login = login;
        }

        private Account(AccountSummary summary)
        {
            Login = summary.Login;
            UUID = summary.UUID;
            IsActive = summary.IsActive;
            IsLaunched = summary.IsLaunched;
            AccountType = summary.AccountType;

            try
            {
                byte[] key = Encoding.UTF8.GetBytes(LaunсherSettings.passwordKey);
                byte[] IV = Encoding.UTF8.GetBytes(LaunсherSettings.passwordKey.Substring(0, 16));
                byte[] decripted = Cryptography.AesDecode(Convert.FromBase64String(summary.AccessData), key, IV);
                _accessData = Encoding.UTF8.GetString(decripted);
            }
            catch (Exception ex)
            {
                _accessData = null;
                Runtime.DebugWrite("Exception " + ex);
            }
        }

        private AccountSummary GetAccountSummary()
        {
            string accessData = null;
            try
            {
                Convert.ToBase64String(Cryptography.AesEncode(this._accessData, Encoding.UTF8.GetBytes(LaunсherSettings.passwordKey), Encoding.UTF8.GetBytes(LaunсherSettings.passwordKey.Substring(0, 16))));
            }
            catch (Exception ex)
            {
                Runtime.DebugWrite("Exception " + ex);
            }

            return new AccountSummary()
            {
                Login = this.Login,
                UUID = this.UUID,
                IsActive = this.IsActive,
                IsLaunched = this.IsLaunched,
                AccountType = this.AccountType,
                AccessData = accessData
            };
        }

        public void Save()
        {
            AddToList(this);
            SaveSummaryList();
        }

        /// <summary>
        /// Проводит авторизацию
        /// </summary>
        /// <param name="newAccessData">
        /// Данные для авторизации (пароль/токен/другая хуйня). 
        /// Передавать null, если нужно переавторизироваться по сохраненным данным.
        /// </param>
        /// <returns>Результат авторизации</returns>
        public AuthCode Auth(string newAccessData)
        {
            IAuthHandler authHandler;
            switch (AccountType)
            {
                case AccountType.NightWorld:
                    authHandler = new NightWorldAuth();
                    break;
                //case AccountType.Mojang:
                //    authHandler = new MojangAuth();
                //    break;
                case AccountType.Microsoft:
                    authHandler = new MicrosoftAuth();
                    break;
                case AccountType.NoAuth:
                    authHandler = new LocalAuth();
                    break;
                default:
                    authHandler = null;
                    break;
            }

            IAuthHandler.AuthResult result;
            if (newAccessData == null)
            {
                result = authHandler.ReAuth(Login, _accessData);
            }
            else
            {
                result = authHandler.Auth(Login, newAccessData);
            }

            if (result.Code == AuthCode.Successfully)
            {
                IsAuthed = true;
                Login = result.Login;
                _accessData = result.AccessData;
                UUID = result.UUID;
                Status = result.Status;
                AccessToken = result.AccessToken;
                SessionToken = result.SessionToken;

                TryInitNwServices();
            }

            return result.Code;
        }

        private string _gameClientName = "";

        private void TryInitNwServices()
        {
            if (AccountType == AccountType.NightWorld && _isActive)
            {
                // запускаем поток который постоянно будет уведомлять сервер о том что мы в сети
                Lexplosion.Runtime.TaskRun(delegate ()
                {
                    while (IsAuthed && _isActive)
                    {
                        ToServer.HttpGet(LaunсherSettings.URL.UserApi + "setActivity?status=" + (int)Status + "&UUID=" + UUID + "&sessionToken=" + SessionToken + "&gameClientName=" + _gameClientName);
                        Thread.Sleep(240000); // Ждём 4 минуты
                    }
                });

                LaunchGame.OnGameProcessStarted += this.GameStart;
                LaunchGame.OnGameStoped += this.GameStop;
                Lexplosion.Runtime.OnExitEvent += this.Exit;
            }
        }

        private void TryStopNwServices()
        {
            if (AccountType == AccountType.NightWorld)
            {
                LaunchGame.OnGameProcessStarted -= this.GameStart;
                LaunchGame.OnGameStoped -= this.GameStop;
                Lexplosion.Runtime.OnExitEvent -= this.Exit;
            }
        }

        private void GameStart(LaunchGame gameManager)
        {
            if (Status == ActivityStatus.Online)
            {
                _gameClientName = gameManager.GameClientName;
                Status = ActivityStatus.InGame;
                ToServer.HttpGet(LaunсherSettings.URL.UserApi + "setActivity?status=2&UUID=" + UUID + "&sessionToken=" + SessionToken + "&gameClientName=" + _gameClientName);
            }
        }

        private void GameStop(LaunchGame gameManager)
        {
            if (Status == ActivityStatus.InGame)
            {
                Status = ActivityStatus.Online;
                ToServer.HttpGet(LaunсherSettings.URL.UserApi + "setActivity?status=1&UUID=" + UUID + "&sessionToken=" + SessionToken);
            }
        }

        private void Exit()
        {
            ToServer.HttpGet(LaunсherSettings.URL.UserApi + "setActivity?status=0&UUID=" + UUID + "&sessionToken=" + SessionToken);
        }

        public void ChangeBaseStatus(ActivityStatus status)
        {
            if (SessionToken != null)
            {
                int statusInt = 0;
                if (status == ActivityStatus.Offline)
                {
                    statusInt = 1;
                }
                else if (status == ActivityStatus.NotDisturb)
                {
                    statusInt = 2;
                }

                ToServer.HttpGet(LaunсherSettings.URL.UserApi + "setBaseStatus?activityStatus=" + statusInt + "&UUID=" + UUID + "&sessionToken=" + SessionToken);
            }

            Status = status;
        }
    }
}

using Lexplosion.Global;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Management.Accounts.Auth;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Lexplosion.Logic.FileSystem;
using Newtonsoft.Json;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Network.Services;
using Lexplosion.Logic.Objects.Nightworld;

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
			public bool IsLaunch;

			public bool IsValid()
			{
				return !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(UUID) && Enum.IsDefined(typeof(AccountType), (int)AccountType);
			}
		}

		public string Login { get; private set; }
		public string UUID { get; private set; }
		public string AccessToken { get; private set; }
		public string SessionToken { get; private set; }
		public NwUserBanner ProfileBanner { get; private set; }
		public AccountType AccountType { get; private set; }

		private ActivityStatus _status = ActivityStatus.Offline;
		public ActivityStatus Status
		{
			get => (IsActive || IsLaunch) ? _status : ActivityStatus.Offline;
			private set
			{
				_status = value;
				OnPropertyChanged();
			}
		}

		private AuthCode _latestAuthCode;
		public AuthCode AuthorizationCodeResult
		{
			get => _latestAuthCode; private set
			{
				_latestAuthCode = value;
				OnPropertyChanged();
			}
		}

		public static string AnyFuckingLogin { get => ActiveAccount?.Login ?? LaunchAccount?.Login; }

		/// <summary>
		/// Событие которые происходит при окончании авторизации.
		/// </summary>
		public static event Action<Account> AuthozationFinished;
		/// <summary>
		/// Событие которые происходит при удалении аккаунта.
		/// </summary>
		public static event Action<Account> AccountDeleted;
		/// <summary>
		/// Событие которое происходит когда меняется активный аккаунт.
		/// </summary>
		public static event Action<Account?> ActiveAccountChanged;
		/// <summary>
		/// Событие которое происходит когда меняется запускаемый аккаунт.
		/// </summary>
		public static event Action<Account?> LaunchAccountChanged;
		/// <summary>
		/// Событие которые происходит когда добавляется при добавлении нового аккаунта.
		/// </summary>
		public static event Action<Account> AccountAdded;

		public bool IsTokenValid { get; private set; } = true;

		private readonly ManualResetEvent _authWait = new ManualResetEvent(false);

		/// <summary>
		/// Ссылка на картинку головы скина. Может быть null.
		/// </summary>
		public string HeadImageUrl
		{
			get
			{
				if (string.IsNullOrWhiteSpace(Login)) return null;
				if (AccountType != AccountType.NightWorld) return $"https://mineskin.eu/helm/{Login}/64.png";
				return $"https://night-world.org/requestProcessing/getUserImage.php?user_login={Login}";
			}
		}

		private static readonly object StateSetLocker = new object();
		private readonly IAuthServicesContainer _services;

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
				lock (StateSetLocker)
				{
					IsActiveSet(value);
				}
			}
		}

		private bool _isActive = false;

		/// <summary>
		/// Является ли аккаунт запускаемым. 
		/// Если установить true, то IsLaunch другого аккаунта, который был запускаемым изменится на false.
		/// При установке true, статичное свойство LaunchedAccount примет значение в виде этого аккаунта.
		/// Так же, если установить true аккаунту NightWorld, то IsActive у этого аккаунта тоже примет значение true.
		/// </summary>
		private bool _isLaunch = false;
		public bool IsLaunch
		{
			get => _isLaunch;
			set
			{
				lock (StateSetLocker)
				{
					IsLaunchSet(value);
				}
			}
		}

		private void IsLaunchSet(bool value)
		{
			if (value)
			{
				// делаем аккаунт запускаемым, и если это аккаунт найтворлд, то делаем его и активным
				SetToLaunch();
				if (AccountType == AccountType.NightWorld) IsActiveSet(true);
			}
			else
			{
				if (LaunchAccount != null && LaunchAccount.Equals(this)) LaunchAccount = null;
				_isLaunch = false;
				OnPropertyChanged(nameof(IsLaunch));
			}
		}

		private void IsActiveSet(bool value)
		{
			// производим какие-либо действия, только если аккаунт найтворлд. отсальные должны всегда иметь false
			if (AccountType == AccountType.NightWorld)
			{
				_isActive = value;
				if (_isActive)
				{
					if (ActiveAccount != null && !ActiveAccount.Equals(this)) // если ActiveAccount не равен null и в ActiveAccount леэит не этот аккаунт
					{
						// если ActiveAccount это найтворлд аккаунт и он является запускаемым, то ActiveAccount делаем не запускаемым, а этот аккаунт запускаемым
						// эта хрень нужна потому что не должно быть так, что один найтворлд аккаунт был запускаемым, а другой активным
						if (ActiveAccount.AccountType == AccountType.NightWorld && ActiveAccount.IsLaunch)
						{
							ActiveAccount.IsLaunchSet(false);
							SetToLaunch();
						}

						ActiveAccount.IsActiveSet(false);
					}

					ActiveAccount = this;
					TryInitNwServices();
				}
				else
				{
					// если в ActiveAccount находится этот аккаунт, то обнуляем ActiveAccount
					if (ActiveAccount != null && ActiveAccount.Equals(this)) ActiveAccount = null;
					TryStopNwServices();
				}

				OnPropertyChanged(nameof(IsActive));
			}
		}

		private void SetToLaunch()
		{
			if (LaunchAccount != null && !LaunchAccount.Equals(this)) LaunchAccount.IsLaunchSet(false);
			LaunchAccount = this;
			_isLaunch = true;
			OnPropertyChanged(nameof(IsLaunch));
		}

		private string _accessData;

		private static Dictionary<Account, AccountSummary> _listToSave;
		private static HashSet<Account> _accounts;

		public bool IsAuthed { get; private set; } = false;
		private bool _isAuthIsProcess;
		public bool IsAuthInProcess
		{
			get => _isAuthIsProcess; private set
			{
				_isAuthIsProcess = value;
				OnPropertyChanged();
			}
		}

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
		/// 
		private static Account _activeAccount;
		public static Account ActiveAccount
		{
			get => _activeAccount; private set
			{
				_activeAccount = value;
				ActiveAccountChanged?.Invoke(value);
			}
		}

		/// <summary>
		/// Возвращает запускаемый аккаунт.
		/// </summary>
		private static Account _launchAccount;
		public static Account LaunchAccount
		{
			get => _launchAccount; private set
			{
				_launchAccount = value;
				LaunchAccountChanged?.Invoke(value);
			}
		}

		public static void Init()
		{
			var list = Runtime.ServicesContainer.DataFilesService.GetFile<HashSet<AccountSummary>>(LaunсherSettings.LauncherDataPath + "/accounts.json");
			_listToSave = new Dictionary<Account, AccountSummary>();
			_accounts = new HashSet<Account>();

			if (list != null)
			{
				foreach (var item in list)
				{
					if (item.IsValid())
					{
						var account = new Account(item, Runtime.ServicesContainer);
						AddToList(account);
					}
				}
			}
			else
			{
				//если список аккаунтов пуст, то получаем список аккаунтов из файла старой версии
				var oldList = Runtime.ServicesContainer.DataFilesService.GetFile<OldAcccountsFormat>(LaunсherSettings.LauncherDataPath + "/account.json");
				if (oldList != null && Enum.IsDefined(typeof(AccountType), (int)oldList.SelectedProfile))
				{
					foreach (var item in oldList.Profiles)
					{
						OldAcccountsFormat.Profile profile = item.Value;
						if (string.IsNullOrWhiteSpace(profile?.Login) || string.IsNullOrWhiteSpace(profile.AccessData) || !Enum.IsDefined(typeof(AccountType), (int)item.Key)) continue;

						var summary = new AccountSummary()
						{
							AccessData = profile.AccessData,
							Login = profile.Login,
							IsActive = item.Key == AccountType.NightWorld,
							IsLaunch = item.Key == oldList.SelectedProfile,
							AccountType = item.Key
						};

						var account = new Account(summary, Runtime.ServicesContainer);
						AddToList(account);
					}

					SaveAll();
				}
			}
		}

		private static void AddToList(Account account)
		{
			_accounts.Add(account);
			_listToSave[account] = account.GetAccountSummary();
			AccountAdded?.Invoke(account);
		}

		private static void SaveSummaryList()
		{
			Runtime.ServicesContainer.DataFilesService.SaveFile(LaunсherSettings.LauncherDataPath + "/accounts.json", JsonConvert.SerializeObject(_listToSave.Values));
		}

		public Account(AccountType type, IAuthServicesContainer services, DataFilesManager dataFilesManager)
		{
			AccountType = type;
			_services = services;
		}

		public Account(AccountType type, IAuthServicesContainer services, DataFilesManager dataFilesManager, string login) : this(type, services, dataFilesManager)
		{
			Login = login;
		}

		private Account(AccountSummary summary, IAuthServicesContainer services)
		{
			AccountType = summary.AccountType;
			Login = summary.Login;
			UUID = summary.UUID;
			IsActive = summary.IsActive;
			IsLaunch = summary.IsLaunch;
			_services = services;

			if (AccountType == AccountType.NoAuth) return;

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
			if (AccountType != AccountType.NoAuth)
			{
				try
				{
					accessData = Convert.ToBase64String(Cryptography.AesEncode(this._accessData, Encoding.UTF8.GetBytes(LaunсherSettings.passwordKey), Encoding.UTF8.GetBytes(LaunсherSettings.passwordKey.Substring(0, 16))));
				}
				catch (Exception ex)
				{
					Runtime.DebugWrite("Exception " + ex);
				}
			}

			return new AccountSummary()
			{
				Login = this.Login,
				UUID = this.UUID,
				IsActive = this.IsActive,
				IsLaunch = this.IsLaunch,
				AccountType = this.AccountType,
				AccessData = accessData
			};
		}

		/// <summary>
		/// Заносит этот аккаунт в список аккаунтов и сохраняется в файл.
		/// </summary>
		public void Save()
		{
			AddToList(this);
			SaveSummaryList();
		}

		/// <summary>
		/// Заносит этот аккаунт в список аккаунтов без сохранения в файл.
		/// </summary>
		public void PutToList() => AddToList(this);

		/// <summary>
		/// Соохраняет все аккаунты из спсика аккаунтов
		/// </summary>
		public static void SaveAll()
		{
			foreach (var account in _accounts)
			{
				_listToSave[account] = account.GetAccountSummary();
			}

			SaveSummaryList();
		}

		/// <summary>
		/// Удаляем аккаунт их списка аккаунтов
		/// </summary>
		public void RemoveFromList()
		{
			IsActive = false;
			IsLaunch = false;

			_accounts.Remove(this);
			_listToSave.Remove(this);

			AccountDeleted?.Invoke(this);
		}

		/// <summary>
		/// Проводит авторизацию
		/// </summary>
		/// <param name="newAccessData">
		/// Данные для авторизации (пароль/токен/другая хуйня). 
		/// Передавать null, если нужно переавторизироваться по сохраненным данным.
		/// </param>
		/// <returns>Результат авторизации</returns>
		public AuthCode Auth(string newAccessData = null)
		{
			IsAuthInProcess = true;
			IAuthHandler authHandler;
			switch (AccountType)
			{
				case AccountType.NightWorld:
					authHandler = new NightWorldAuth(_services.NwApi);
					break;
				//case AccountType.Mojang:
				//    authHandler = new MojangAuth();
				//    break;
				case AccountType.Microsoft:
					authHandler = new MicrosoftAuth(_services.MjApi);
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
				OnPropertyChanged(nameof(IsAuthed));
				Login = result.Login;
				OnPropertyChanged(nameof(Login));
				_accessData = result.AccessData;
				UUID = result.UUID;
				Status = result.Status;
				AccessToken = result.AccessToken;
				SessionToken = result.SessionToken;
				ProfileBanner = result.AdditionalInfo as NwUserBanner;

                TryInitNwServices();
			}

			if (result.Code != AuthCode.Successfully)
			{
				IsActive = false;
			}

			AuthozationFinished?.Invoke(this);
			AuthorizationCodeResult = result.Code;
			IsAuthInProcess = false;
            _authWait.Set();

            Runtime.DebugWrite($"Auth {Login}, Account type: {AccountType}, Result: {result.Code}");
			return result.Code;
		}

		public void WaitAuth()
		{
			_authWait.WaitOne();
		}

		private string _gameClientName = "";
		private bool _nwServicesIsInit = false;

		private void Exit()
		{
			_services.WebService.HttpGet(LaunсherSettings.URL.UserApi + "setActivity?status=0&UUID=" + UUID + "&sessionToken=" + SessionToken);
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

				_services.WebService.HttpGet(LaunсherSettings.URL.UserApi + "setBaseStatus?activityStatus=" + statusInt + "&UUID=" + UUID + "&sessionToken=" + SessionToken);
			}

			Status = status;
		}


		private void TryInitNwServices()
		{
			if (AccountType == AccountType.NightWorld && _isActive)
			{
				// запускаем поток который постоянно будет уведомлять сервер о том что мы в сети
				Lexplosion.Runtime.TaskRun(delegate ()
				{
					while (IsAuthed && _isActive)
					{
						_services.WebService.HttpGet(LaunсherSettings.URL.UserApi + "setActivity?status=" + (int)Status + "&UUID=" + UUID + "&sessionToken=" + SessionToken + "&gameClientName=" + _gameClientName);
						Thread.Sleep(240000); // Ждём 4 минуты
					}
				});

				Runtime.OnExitEvent += this.Exit;
				_nwServicesIsInit = true;
			}
		}

		private void TryStopNwServices()
		{
			if (AccountType == AccountType.NightWorld && _nwServicesIsInit)
			{
				Runtime.OnExitEvent -= this.Exit;
				_nwServicesIsInit = false;
				ThreadPool.QueueUserWorkItem((object obj) => Exit());
			}
		}

		public void SetInGameStatus(string gameClientName)
		{
			if (AccountType == AccountType.NightWorld && IsAuthed && _nwServicesIsInit)
			{
				_gameClientName = gameClientName;
				Status = ActivityStatus.InGame;
				_services.WebService.HttpGet(LaunсherSettings.URL.UserApi + "setActivity?status=2&UUID=" + UUID + "&sessionToken=" + SessionToken + "&gameClientName=" + _gameClientName);
			}
		}

		public void SetOnlineStatus()
		{
			if (AccountType == AccountType.NightWorld && IsAuthed && _nwServicesIsInit)
			{
				Status = ActivityStatus.Online;
				_services.WebService.HttpGet(LaunсherSettings.URL.UserApi + "setActivity?status=1&UUID=" + UUID + "&sessionToken=" + SessionToken);
			}
		}
	}
}

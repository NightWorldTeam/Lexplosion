using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Network.WebSockets;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Network.Services;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Objects;

namespace Lexplosion
{
	public static partial class Runtime
	{
		public static bool IsFirtsLaunch { get; private set; }
		public static Process CurrentProcess { get; private set; }

		public static AllServicesContainer ServicesContainer { get; private set; }
		public static ClientsManager ClientsManager { get; private set; }

		/// <summary>
		/// Происходит переда закрытием лаунчера
		/// </summary>
		public static event Action OnExitEvent;
		/// <summary>
		/// Происходит перед запуском процесса обновления лаунчера
		/// </summary>
		public static event Action OnUpdateStart;
		/// <summary>
		/// Происходит если была запущена вторая копия лаунчера
		/// </summary>
		public static event Action OnLexplosionOpened;
		/// <summary>
		/// Происходит если было вызвано закрытие лаунчера методом Exit
		/// </summary>
		public static event Action ПереходВРежимЗавершения;

		private static Mutex? InstanceCheckMutex;

		/// <summary>
		/// Проверяет запущен ли уже лаунчер.
		/// </summary>
		/// <returns>true - нет запущенного экземпляра. false - есть</returns>
		private static bool InstanceCheck()
		{
			var mutex = new Mutex(true, "NW-Lexplosion_Is_launched", out bool isNew);
			if (isNew)
				InstanceCheckMutex = mutex;
			else
				mutex.Dispose();

#if DEBUG
			return true;
#else

			return isNew;
#endif
		}

		public static void InitializedSystem(int updaterOffsetLeft, int updaterOffsetRight)
		{
			//подписываемся на эвент вылета, чтобы логировать все необработанные исключения
			AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs args)
			{
				Exception exception = (Exception)args.ExceptionObject;
				File.WriteAllText(LaunсherSettings.LauncherDataPath + "/crash-report_" + DateTime.Now.ToString("dd.MM.yyyy-h.mm.ss") + ".log", exception.ToString());
			};

			var withDirectory = new WithDirectory();
			var dataFilesManager = new DataFilesManager(withDirectory);
			var toServer = new ToServer();
			var minecraftInfo = new MinecraftInfoService(toServer);
			var nightWorldApi = new NightWorldApi(toServer);
			var modrinthApi = new ModrinthApi(toServer);
			var curesforgeApi = new CurseforgeApi(toServer);
			var mojangApi = new MojangApi(toServer);

			var categoriesManager = new CategoriesManager(modrinthApi, curesforgeApi);

			var services = new AllServicesContainer(toServer, minecraftInfo, withDirectory, dataFilesManager, curesforgeApi, modrinthApi, nightWorldApi, mojangApi, categoriesManager);
			ServicesContainer = services;

			ClientsManager = new ClientsManager(services);

			// инициализация
			GlobalData.InitSetting(dataFilesManager);
			withDirectory.Create(GlobalData.GeneralSettings.GamePath);

			LaunchGame.OnlineGameSystemStarted += AddImportantTask;
			LaunchGame.OnlineGameSystemStoped += RemoveImportantTask;

			CurrentProcess = Process.GetCurrentProcess();

			// Проверяем запущен ли лаунчер.
			if (!InstanceCheck())
			{
				WebSocketClient ws = new WebSocketClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 54352));
				//отправляем уже запущеному лаунчеру запрос о том, что надо бы блять что-то сделать, а то юзер новый запустить пытается
				ws.SendData("$lexplosionOpened:" + CurrentProcess.Id);
				CurrentProcess.Kill(); //стопаем этот процесс
			}

			int version = nightWorldApi.CheckLauncherUpdates();
			if (version == -1)
			{
				var proxies = ProxyFetcher.GetProxies();
				if (proxies.Count > 0)
				{
					toServer.ChangeToProxyMode();
					withDirectory.ChangeDownloadToProxyMode();

					var waiter = new ManualResetEvent(false);
					ProxyFetcher.FindWorkingProxy(proxies, (Proxy proxy) =>
					{
						if (proxy.CalculatedDelay < 0) return;

						toServer.AddProxy(proxy);
						withDirectory.AddProxy(proxy);

						waiter.Set();
					}, () => waiter.Set());

					waiter.WaitOne();

					version = nightWorldApi.CheckLauncherUpdates(15000);

					if (version > LaunсherSettings.version)
					{
						OnUpdateStart?.Invoke();
						LauncherUpdate(version, updaterOffsetLeft, updaterOffsetRight);
					}
				}
			}
			else if (version > LaunсherSettings.version)
			{
				OnUpdateStart?.Invoke();
				LauncherUpdate(version, updaterOffsetLeft, updaterOffsetRight);
			}

			Account.Init();

			ClientsManager.DefineInstalledInstances();

			bool isStarted = CommandReceiver.StartCommandServer();
			if (!isStarted)
			{
				Lexplosion.Runtime.TaskRun(() =>
				{
					bool isWorld;
					do
					{
						isWorld = !CommandReceiver.StartCommandServer();
						Thread.Sleep(2000);
					}
					while (isWorld);
				});
			}

			IsFirtsLaunch = GlobalData.GeneralSettings.ItIsNotShit != true;
			if (IsFirtsLaunch)
			{
				// При сохранении он автоматом пометит ItIsNotShit как true
				dataFilesManager.SaveSettings(GlobalData.GeneralSettings);
			}

			//подписываемся на эвент открытия второй копии лаунчера
			CommandReceiver.OnLexplosionOpened += OnLexplosionOpened;
		}

		private static bool LauncherUpdate(int version, int updaterOffsetLeft, int updaterOffsetRight)
		{
			try
			{
				int upgradeToolVersion = Int32.Parse(ServicesContainer.WebService.HttpPost(LaunсherSettings.URL.LauncherParts + "upgradeToolVersion.html"));
				string gamePath = GlobalData.GeneralSettings.GamePath;

				// скачивание и проверка версии UpgradeTool.exe
				using (WebClient wc = new WebClient())
				{
					wc.Proxy = null;
					if (ServicesContainer.DataFilesService.GetUpgradeToolVersion() < upgradeToolVersion && File.Exists(gamePath + "/UpgradeTool.exe"))
					{
						File.Delete(gamePath + "/UpgradeTool.exe");
						wc.DownloadFile(LaunсherSettings.URL.LauncherParts + "UpgradeTool.exe?" + upgradeToolVersion, gamePath + "/UpgradeTool.exe");
						ServicesContainer.DataFilesService.SetUpgradeToolVersion(upgradeToolVersion);

					}
					else if (!File.Exists(gamePath + "/UpgradeTool.exe"))
					{
						wc.DownloadFile(LaunсherSettings.URL.LauncherParts + "UpgradeTool.exe?" + upgradeToolVersion, gamePath + "/UpgradeTool.exe");
						ServicesContainer.DataFilesService.SetUpgradeToolVersion(upgradeToolVersion);
					}
				}

				var arguments =
					"\"" + Assembly.GetEntryAssembly().Location + "\" " +
					"\"" + LaunсherSettings.URL.LauncherParts + "Lexplosion.exe?" + version + "\" " +
					Process.GetCurrentProcess().Id + " " +
					Convert.ToInt32(updaterOffsetLeft) + " " +
					Convert.ToInt32(updaterOffsetRight);

				// запуск UpgradeTool.exe
				Process proc = new Process();
				proc.StartInfo.FileName = gamePath + "/UpgradeTool.exe";
				proc.StartInfo.Arguments = arguments;
				proc.Start();

				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Закрытие лаунчера. В отличии от Exit сразу убивает процесс лаунчера, не дожидаясь заврешения приоритетных задач.
		/// </summary>
		public static void KillApp()
		{
			BeforeExit(null, null);
			Environment.Exit(0);
		}

		/// <summary>
		/// Закрытие лаунчера. Если запущена приоритетная задача, то ждет её завршения и только потом закрывеат лаунчер. 
		/// Закртие может быть отменено методом CancelingExit
		/// </summary>
		public static void Exit()
		{
			Runtime.DebugWrite("Exit");
			lock (_locker)
			{
				_inExited = true;

				if (_importantThreads > 0)
				{
					ПереходВРежимЗавершения?.Invoke();
				}
			}

			TaskRun(delegate ()
			{
				_waitingClosing.WaitOne(); // ждём отработки всех приоритетных задач. 
										   // проверяем было ли закрытие отменено
				if (_exitIsCanceled)
				{
					// снова блочим waitingClosing, если сохранилась приоритетная задача, ибо метод CancelExit ее разлочил
					lock (_locker)
					{
						if (_importantThreads > 0)
						{
							_waitingClosing.Reset();
						}
					}

					_exitIsCanceled = false;
					_inExited = false;

					return;
				}

				BeforeExit(null, null);
				Environment.Exit(0);
			});
		}

		public static void BeforeExit(object sender, EventArgs e)
		{
			OnExitEvent?.Invoke();
			CommandReceiver.StopCommandServer();
			FileDistributor.StopWork();
		}

		/// <summary>
		/// Отмена закрытия лаунчера, вызванного методом Exit.
		/// </summary>
		public static void CancelingExit()
		{
			lock (_locker)
			{
				if (_inExited)
				{
					_exitIsCanceled = true;
					_waitingClosing.Set();
				}
			}
		}

		public static void AXAXA()
		{
			TaskRun(() =>
			{
				ThreadPool.GetMaxThreads(out int workerThreads, out int completionPortThreads);
				for (int i = 0; i < workerThreads; i++)
				{
					ThreadPool.QueueUserWorkItem((_) =>
					{
						Thread.Sleep(60 * 60 * 365);
					});
				}
			});
		}
	}
}

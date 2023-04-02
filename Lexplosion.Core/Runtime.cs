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
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;

namespace Lexplosion
{
    static partial class Runtime
    {
        public static Process CurrentProcess { get; private set; }

        public static event Action OnExitEvent;
        public static event Action OnUpdateStart;
        public static event Action OnLexplosionOpened;
        public static event Action ПереходВРежимЗавершения;

        private static Mutex? InstanceCheckMutex;

        /// <summary>
        /// Проверяет запущен ли уже лаунчер.
        /// </summary>
        /// <returns>true - нет запущенного экземпляра. false - есть</returns>
        private static bool InstanceCheck()
        {
            bool isNew;
            var mutex = new Mutex(true, "NW-Lexplosion_Is_launched", out isNew);
            if (isNew)
                InstanceCheckMutex = mutex;
            else
                mutex.Dispose();

            return isNew;
        }

        public static void InitializedSystem(int updaterOffsetLeft, int updaterOffsetRight)
        {
            //подписываемся на эвент вылета, чтобы логировать все необработанные исключения
            AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs args)
            {
                Exception exception = (Exception)args.ExceptionObject;
                DataFilesManager.SaveFile(LaunсherSettings.LauncherDataPath + "/crash-report_" + DateTime.Now.ToString("dd.MM.yyyy-h.mm.ss") + ".log", exception.ToString());
            };

            // инициализация
            GlobalData.InitSetting();
            WithDirectory.Create(GlobalData.GeneralSettings.GamePath);

            CurrentProcess = Process.GetCurrentProcess();

            // Проверяем запущен ли лаунчер.
            if (!InstanceCheck())
            {
                WebSocketClient ws = new WebSocketClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 54352));
                //отправляем уже запущщеному лаунчеру запрос о том, что надо бы блять что-то сделать, а то юзер новый запустить пытается
                ws.SendData("$lexplosionOpened:" + CurrentProcess.Id);
                CurrentProcess.Kill(); //стопаем этот процесс
            }

            int version = ToServer.CheckLauncherUpdates();
            if (version != -1)
            {
                OnUpdateStart?.Invoke();
                LauncherUpdate(version, updaterOffsetLeft, updaterOffsetRight);
            }

            InstanceClient.DefineInstalledInstances();

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

            //подписываемся на эвент открытия второй копии лаунчера
            CommandReceiver.OnLexplosionOpened += OnLexplosionOpened;
        }

        private static bool LauncherUpdate(int version, int updaterOffsetLeft, int updaterOffsetRight)
        {
            try
            {
                int upgradeToolVersion = Int32.Parse(ToServer.HttpPost(LaunсherSettings.URL.LauncherParts + "upgradeToolVersion.html"));
                string gamePath = GlobalData.GeneralSettings.GamePath;

                // скачивание и проверка версии UpgradeTool.exe
                using (WebClient wc = new WebClient())
                {
                    if (DataFilesManager.GetUpgradeToolVersion() < upgradeToolVersion && File.Exists(gamePath + "/UpgradeTool.exe"))
                    {
                        File.Delete(gamePath + "/UpgradeTool.exe");
                        wc.DownloadFile(LaunсherSettings.URL.LauncherParts + "UpgradeTool.exe?" + upgradeToolVersion, gamePath + "/UpgradeTool.exe");
                        DataFilesManager.SetUpgradeToolVersion(upgradeToolVersion);

                    }
                    else if (!File.Exists(gamePath + "/UpgradeTool.exe"))
                    {
                        wc.DownloadFile(LaunсherSettings.URL.LauncherParts + "UpgradeTool.exe?" + upgradeToolVersion, gamePath + "/UpgradeTool.exe");
                        DataFilesManager.SetUpgradeToolVersion(upgradeToolVersion);
                    }
                }

                string arguments =
                    "\"" + Assembly.GetExecutingAssembly().Location + "\" " +
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
        /// убивает процесс лаунчера
        /// </summary>
        public static void KillApp()
        {
            BeforeExit(null, null);
            Environment.Exit(0);
        }

        /// <summary>
        /// Выход из лаунчера. Если запущен приоритетный процесс, то ждет его завршения и только потом закрывеат лаунчер. 
        /// Закртие может быть отменено методом CancelingExit
        /// </summary>
        public static void Exit()
        {
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
        }

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
    }
}

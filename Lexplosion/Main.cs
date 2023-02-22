using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Threading;
using System.IO.Compression;
using System.Windows.Media;
using System.Collections.Generic;
using System.Text;
using Hardcodet.Wpf.TaskbarNotification;
using DiscordRPC;
using Lexplosion.Properties;
using Lexplosion.Global;
using Lexplosion.Tools;
using Lexplosion.Gui.Views.Windows;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Network.WebSockets;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;

using ConsoleWindow = Lexplosion.Gui.Views.Windows.Console;
using ColorConverter = System.Windows.Media.ColorConverter;

/*
 * Лаунчер Lexplosion. Разработано NightWorld Team.
 * Никакие права не защищены.
 * Главный исполняемый файл лаунчера. Здесь людей ебут
 */

namespace Lexplosion
{
    static partial class Runtime
    {
        private static App app = new App();
        private static SplashWindow _splashWindow;

        private static TaskbarIcon _nofityIcon;

        public static Process CurrentProcess { get; private set; }

        public static event Action ExitEvent;
        public static event Action TrayMenuElementClicked;

        // TODO: сохранять координаты закрытого главного окна, использовать при открытии следующего

        private static double leftPos;
        private static double topPos;

        const string AssetsPath = "pack://application:,,,/Assets/";
        const string ResourcePath = "pack://application:,,,/Gui/Resources/";
        public const string LangPath = AssetsPath + "langs/";

        public static readonly string[] Languages = new string[]
        {
            "ru-RU", "en-US"
        };

        private static ResourceDictionary CurrentLangDict;

        public static Color CurrentAccentColor => (Color)app.Resources["ActivityColor"];
        public static Color[] AccentColors;

        [STAThread]
        static void Main()
        {
            // Подписываемся на эвент для загрузки всех строенных dll'ников
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            app.Exit += BeforeExit;

            Thread thread = new Thread(InitializedSystem);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            _splashWindow = new SplashWindow();
            _splashWindow.ChangeLoadingBoardPlaceholder();
            app.Run(_splashWindow);
        }

        private static void SetMainWindow()
        {
            _nofityIcon = (TaskbarIcon)app.FindResource("NofityIcon");
            app.MainWindow.Topmost = true;

            var mainWindow = new MainWindow()
            {
                Left = app.MainWindow.Left - 322,
                Top = app.MainWindow.Top - 89
            };

            leftPos = mainWindow.Left;
            topPos = mainWindow.Top;

            mainWindow.Show();
            ((SplashWindow)app.MainWindow).SmoothClosing();
            app.MainWindow = mainWindow;
        }

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

        private static void InitializedSystem()
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

            // Выставляем язык
            ChangeCurrentLanguage();
            // Встраеваем стили
            app.Dispatcher.Invoke(StylesInit);

            CurrentProcess = Process.GetCurrentProcess();

            // Проверяем запущен ли лаунчер.
            if (false & !InstanceCheck())
            {
                WebSocketClient ws = new WebSocketClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 54352));
                //отправляем уже запущщеному лаунчеру запрос о том, что надо бы блять что-то сделать, а то юзер новый запустить пытается
                ws.SendData("$lexplosionOpened:" + CurrentProcess.Id);
                CurrentProcess.Kill(); //стопаем этот процесс
            }

            int version = ToServer.CheckLauncherUpdates();
            if (version != -1)
            {
                app.Dispatcher.Invoke(() =>
                {
                    _splashWindow.ChangeLoadingBoardPlaceholder(true);
                });
                LauncherUpdate(version);
            }

            InstanceClient.DefineInstalledInstances();

            bool isStarted = CommandReceiver.StartCommandServer();
            if (!isStarted)
            {
                TaskRun(() =>
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

            var discordClient = InitDiscordApp();

            //подписываемся на эвент открытия второй копии лаунчера
            CommandReceiver.LexplosionOpened += ShowMainWindow;

            LaunchGame.GameStartedEvent += delegate (LaunchGame gameManager) //подписываемся на эвент запуска игры
            {
                // если в настрйоках устанавлено что нужно скрывать лаунчер при запуске клиента, то скрывеам главное окно
                if (GlobalData.GeneralSettings.IsHiddenMode == true)
                {
                    CloseMainWindow();
                }

                discordClient?.SetPresence(new RichPresence()
                {
                    State = "Minecraft " + gameManager.GameVersion,
                    Details = "Сборка " + gameManager.GameClientName,
                    Timestamps = Timestamps.Now,
                    Assets = new Assets()
                    {
                        LargeImageKey = "logo1"
                    }
                });
            };

            // подписываемся на запуск игры до запуска окна
            LaunchGame.GameStartEvent += (LaunchGame gameManager) =>
            {
                if (gameManager.ClientSettings.IsShowConsole == true)
                {
                    app.Dispatcher.Invoke(() => ConsoleWindow.SetWindow(gameManager));
                }
            };

            LaunchGame.GameStopEvent += delegate (LaunchGame gameManager) //подписываемся на эвент завершения игры
            {
                // если в настрйоках устанавлено что нужно скрывать лаунчер при запуске клиента, то показываем главное окно
                if (gameManager.ClientSettings.IsHiddenMode == true)
                {
                    ShowMainWindow();
                }

                discordClient?.SetPresence(new RichPresence()
                {
                    State = "Minecraft не запущен",
                    Timestamps = Timestamps.Now,
                    Assets = new Assets()
                    {
                        LargeImageKey = "logo1"
                    }
                });
            };

            Thread.Sleep(800);

            app.Dispatcher.Invoke(SetMainWindow);
            _splashWindow = null;
        }

        private static void LauncherUpdate(int version)
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
                        wc.DownloadFile(LaunсherSettings.URL.LauncherParts + "UpgradeTool.exe?3", gamePath + "/UpgradeTool.exe");
                        DataFilesManager.SetUpgradeToolVersion(upgradeToolVersion);

                    }
                    else if (!File.Exists(gamePath + "/UpgradeTool.exe"))
                    {
                        wc.DownloadFile(LaunсherSettings.URL.LauncherParts + "UpgradeTool.exe?3", gamePath + "/UpgradeTool.exe");
                        DataFilesManager.SetUpgradeToolVersion(upgradeToolVersion);
                    }

                }

                string arguments = null;

                app.Dispatcher.Invoke(() =>
                {
                    arguments =
                    Assembly.GetExecutingAssembly().Location + " " +
                    LaunсherSettings.URL.LauncherParts + "Lexplosion.exe?" + version + " " +
                    Process.GetCurrentProcess().ProcessName + " " +
                    Convert.ToInt32(_splashWindow.Left) + " " +
                    Convert.ToInt32(_splashWindow.Top);
                });

                // запуск UpgradeTool.exe
                Process proc = new Process();
                proc.StartInfo.FileName = gamePath + "/UpgradeTool.exe";
                proc.StartInfo.Arguments = arguments;
                proc.Start();
            }
            catch
            {
                MessageBox.Show("Не удалось обновить лаунчер!");
            }
        }

        private static byte[] UnzipBytesArray(byte[] zipBytes)
        {
            using (Stream archivedBytes = new MemoryStream(zipBytes))
            {
                using (var zip = new ZipArchive(archivedBytes, ZipArchiveMode.Read))
                {
                    var entry = zip.Entries[0];
                    using (Stream stream = entry.Open())
                    {
                        using (MemoryStream fileBytes = new MemoryStream())
                        {
                            stream.CopyTo(fileBytes);
                            return fileBytes.ToArray();
                        }
                    }
                }
            }
        }

        private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Runtime.DebugWrite("DLL LOAD " + string.Join(", ", args.Name));

            if (args.Name.Contains("Newtonsoft.Json"))
            {
                return Assembly.Load(UnzipBytesArray(Resources.NewtonsoftJson_zip));
            }

            if (args.Name.Contains("LumiSoft.Net"))
            {
                return Assembly.Load(UnzipBytesArray(Resources.LumiSoftNet_zip));
            }

            if (args.Name.Contains("Tommy"))
            {
                return Assembly.Load(UnzipBytesArray(Resources.Tommy_zip));
            }

            if (args.Name.Contains("Hardcodet.Wpf.TaskbarNotification"))
            {
                return Assembly.Load(UnzipBytesArray(Resources.TaskbarNotification_zip));
            }

            if (args.Name.Contains("DiscordRPC"))
            {
                return Assembly.Load(UnzipBytesArray(Resources.DiscordRPC_zip));
            }

            if (args.Name.Contains("System.IO.Compression"))
            {
                return Assembly.Load(Resources.Compression);
            }

            return null;
        }

        public static void ChangeCurrentLanguage(string cultureName = "", bool isRestart = false)
        {
            if (CurrentLangDict == null) CurrentLangDict = new ResourceDictionary();

            if (cultureName.Length == 0)
            {
                try
                {
                    if (GlobalData.GeneralSettings.LanguageId.Length == 0)
                    {
                        var currentCultureName = Thread.CurrentThread.CurrentCulture.ToString();
                        //switch () тут код для стран cis
                        CurrentLangDict.Source = new Uri(LangPath + currentCultureName + ".xaml");
                        GlobalData.GeneralSettings.LanguageId = currentCultureName;
                        DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
                    }
                    else
                    {
                        CurrentLangDict.Source = new Uri(LangPath + GlobalData.GeneralSettings.LanguageId + ".xaml");
                    }
                }
                catch
                {
                    CurrentLangDict.Source = new Uri(LangPath + "ru-RU.xaml");
                    GlobalData.GeneralSettings.LanguageId = "ru-RU";
                    DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
                }
            }
            app.Resources.MergedDictionaries.Add(CurrentLangDict);

            if (cultureName.Length != 0 && isRestart)
            {
                Process.Start(Application.ResourceAssembly.Location);
                App.Current.Shutdown();
            }
        }

        /// <summary>
        /// Иницализация стилей приложения.
        /// </summary>
        private static void StylesInit()
        {
            var colorDict = new ResourceDictionary() { Source = new Uri(ResourcePath + "Colors.xaml") };

            var i = 0;
            var isRightColor = false;
            var accentColorsList = new List<Color>();
            foreach (var resourceKey in colorDict.Keys)
            {
                var strResourceKey = (String)resourceKey;
                if (strResourceKey.Contains("Accent"))
                {
                    var color = (Color)colorDict[resourceKey];
                    accentColorsList.Add(color);
                    try
                    {
                        if (GlobalData.GeneralSettings.AccentColor.Length == 7 && !isRightColor)
                            isRightColor = color.ToString() == ((Color)ColorConverter.ConvertFromString(GlobalData.GeneralSettings.AccentColor)).ToString();
                    }
                    catch { }
                    i++;
                }
            }
            AccentColors = accentColorsList.ToArray();

            app.Resources.MergedDictionaries.Add(colorDict);


            if (GlobalData.GeneralSettings.AccentColor.Length == 0 || !isRightColor)
            {
                ChangeColorToColor((Color)app.Resources["ActivityColor"]);
            }
            else
            {
                ChangeColorToColor((Color)ColorConverter.ConvertFromString(GlobalData.GeneralSettings.AccentColor));
            }

            app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(ResourcePath + "Fonts.xaml")
            });
            app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(ResourcePath + "Iconics.xaml")
            });
            app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(ResourcePath + "Defaults.xaml")
            });
            app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(ResourcePath + "TextBoxStyles.xaml")
            });
            app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(ResourcePath + "TabControlStyles.xaml")
            });
            app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(ResourcePath + "ButtonStyles.xaml")
            });
            app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(ResourcePath + "ListboxStyles.xaml")
            });
            app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(ResourcePath + "StylesDictionary.xaml")
            });
            app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(ResourcePath + "ComboBoxStyles.xaml")
            });
            app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Controls/Controls.xaml")
            });
            app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/DataTemplates.xaml")
            });
        }

        public static void ChangeColorToColor(Color color)
        {
            app.Resources["ActivityColor"] = color;
            app.Resources["BrandSolidColorBrush"] = new SolidColorBrush(color);
            GlobalData.GeneralSettings.AccentColor = ColorTools.FromRgbToHex(color.R, color.G, color.B);
            DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
        }

        private static DiscordRpcClient InitDiscordApp()
        {
            DiscordRpcClient client = new DiscordRpcClient(LaunсherSettings.DiscordAppID);

            if (client.Initialize())
            {
                client.SetPresence(new RichPresence()
                {
                    State = "Minecraft не запущен",
                    Timestamps = Timestamps.Now,
                    Assets = new Assets()
                    {
                        LargeImageKey = "logo1"
                    },
                    Party = new Party()
                    {
                        ID = "Tedsfd",
                        Max = 4,
                        Size = 2,
                        Privacy = Party.PrivacySetting.Public
                    }
                });

                return client;
            }

            return null;
        }

        public static void TrayMenuElementClickExecute()
        {
            TrayMenuElementClicked?.Invoke();
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
        /// Выход из лаунчера. Если запущен приоритетный процесс, то ждет его завршения и только потом закрывеат лаунчер. Закртие может быть омтенено методом ShowMainWindow
        /// </summary>
        public static void Exit()
        {
            lock (locker)
            {
                _inExited = true;

                if (importantThreads > 0)
                {
                    CloseMainWindow();
                }
            }

            TaskRun(delegate ()
            {
                waitingClosing.WaitOne(); // ждём отработки всех приоритетных задач. 
                                          // проверяем было ли закрытие отменено
                if (_exitIsCanceled)
                {
                    // снова блочим waitingClosing, если сохранилась приоритетная задача, ибо метод CancelExit ее разлочил
                    lock (locker)
                    {
                        if (importantThreads > 0)
                        {
                            waitingClosing.Reset();
                        }
                    }

                    _exitIsCanceled = false;
                    _inExited = false;

                    return;
                }

                app.Dispatcher.Invoke(delegate ()
                {
                    BeforeExit(null, null);
                    Environment.Exit(0);
                });
            });
        }

        public static void BeforeExit(object sender, EventArgs e)
        {
            //закрываем все окна
            foreach (Window window in app.Windows)
            {
                window.Close();
            }

            _nofityIcon.Dispose();
            ExitEvent?.Invoke();
            CommandReceiver.StopCommandServer();
        }

        public static void ShowMainWindow()
        {
            lock (locker)
            {
                if (_inExited)
                {
                    _exitIsCanceled = true;
                    waitingClosing.Set();
                }
            }

            app.Dispatcher.Invoke(() =>
            {
                if (app.MainWindow == null)
                {
                    app.MainWindow = new MainWindow()
                    {
                        Left = leftPos,
                        Top = topPos
                    };
                    app.MainWindow.Show();
                }
                else
                {
                    NativeMethods.ShowProcessWindows(CurrentProcess.MainWindowHandle);
                }
            });
        }

        public static void CloseMainWindow()
        {
            app.Dispatcher.Invoke(() =>
            {
                if (app.MainWindow != null)
                {
                    leftPos = app.MainWindow.Left;
                    topPos = app.MainWindow.Top;
                    app.MainWindow.Close();
                    app.MainWindow = null;
                }
            });
        }
    }
}

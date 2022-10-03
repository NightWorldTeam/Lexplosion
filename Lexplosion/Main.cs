using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Threading;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using Lexplosion.Properties;
using Lexplosion.Global;
using Lexplosion.Tools;
using Lexplosion.Gui.Views.Windows;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using System.Collections.Generic;
using Hardcodet.Wpf.TaskbarNotification;
using Lexplosion.Gui;

/*
 * Лаунчер Lexplosion. Создано NightWorld Team в 2019 году.
 * Последнее обновление в апреле 2022 года
 * Главный исполняемый файл лаунчера. Здесь людей ебут
 */

namespace Lexplosion
{
    static class Runtime
    {
        private static App app = new App();
        private static SplashWindow _splashWindow;

        public static Process CurrentProcess { get; private set; }

        public static event Action ExitEvent;

        public static event Action TrayMenuElementClicked;

        [STAThread]
        static void Main()
        {
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            app.Exit += BeforeExit;

            Thread thread = new Thread(InitializedSystem);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            _splashWindow = new SplashWindow();
            _splashWindow.ChangeLoadingBoardPlaceholder();
            app.Run(_splashWindow);
        }

        public static void TrayMenuElementClickExecute()
        {
            TrayMenuElementClicked?.Invoke();
        }

        private static void InitializedSystem()
        {
            //подписываемся на эвент вылета, чтобы логировать все необработанные исключения
            AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs args)
            {
                Exception exception = (Exception)args.ExceptionObject;
                DataFilesManager.SaveFile(LaunсherSettings.LauncherDataPath + "/crash-report_" + DateTime.Now.ToString("dd.MM.yyyy-h.mm.ss") + ".log", exception.ToString());
            };

            // Подписываемся на эвент для загрузки всех строенных dll'ников
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

            // получем процессы с таким же именем (то есть пытаемся получить уже запущенную копию лаунчера)
            Process[] procs = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            CurrentProcess = Process.GetCurrentProcess();

            // процессов больше одного. Знчит лаунечр уже запущен
            if (procs.Length > 1)
            {
                // делаем окно уже запущенного лаунечра активным
                foreach (Process proc in procs)
                {
                    if (proc.Id != CurrentProcess.Id)
                    {
                        NativeMethods.ShowProcessWindows(proc.MainWindowHandle);
                    }
                }

                CurrentProcess.Kill(); //стопаем процесс
            }

            // Встраеваем стили
            StylesInit();

            // инициализация
            UserData.InitSetting();
            WithDirectory.Create(UserData.GeneralSettings.GamePath);

            if (ToServer.CheckLauncherUpdates())
            {
                app.Dispatcher.Invoke(() =>
                {
                    _splashWindow.ChangeLoadingBoardPlaceholder(true);
                });
                LauncherUpdate();
            }

            InstanceClient.DefineInstalledInstances();
            CommandReceiver.StartCommandServer();

            LaunchGame.GameStartedEvent += delegate () //подписываемся на эвент запуска игры
            {
                // если в настрйоках устанавлено что нужно скрывать лаунчер при запуске клиента, то скрывеам главное окно
                if (UserData.GeneralSettings.HiddenMode == true)
                {
                    CloseMainWindow();
                }
            };

            // подписываемся на запуск игры до запуска окна
            LaunchGame.GameStartEvent += (LaunchGame gameManager) =>
            {
                if (UserData.GeneralSettings.ShowConsole == true)
                {
                    app.Dispatcher.Invoke(() =>
                    {
                        var console = new Gui.Views.Windows.Console(gameManager)
                        {
                            Left = app.MainWindow.Left - 322,
                            Top = app.MainWindow.Top - 89
                        };

                        console.Show();
                    });
                }
            };

            LaunchGame.GameStopEvent += delegate (LaunchGame gameManager) //подписываемся на эвент завершения игры
            {
                // если в настрйоках устанавлено что нужно скрывать лаунчер при запуске клиента, то показываем главное окно
                if (UserData.GeneralSettings.HiddenMode == true)
                {
                    ShowMainWindow();
                }

                //if (UserData.GeneralSettings.ShowConsole == true)
                //{
                //    app.Dispatcher.Invoke(() =>
                //    {
                //        ConsoleList[str].Exit(null, null);
                //        ConsoleList.Remove(str);
                //    });
                //}
            };

            Thread.Sleep(800);

            app.Dispatcher.Invoke(() =>
            {
                nofityIcon = (TaskbarIcon)app.FindResource("NofityIcon");
                app.MainWindow.Topmost = true;

                var mainWindow = new Gui.Views.Windows.MainWindow()
                {
                    Left = app.MainWindow.Left - 322,
                    Top = app.MainWindow.Top - 89
                };

                mainWindow.Show();
                ((Gui.Views.Windows.SplashWindow)app.MainWindow).SmoothClosing();
                app.MainWindow = mainWindow;
            });

            _splashWindow = null;
        }

        private static void LauncherUpdate()
        {
            //try
            {
                int upgradeToolVersion = Int32.Parse(ToServer.HttpPost(LaunсherSettings.URL.LauncherParts + "upgradeToolVersion.html"));
                string gamePath = UserData.GeneralSettings.GamePath;

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
                    LaunсherSettings.URL.LauncherParts + "Lexplosion.exe" + " " +
                    Process.GetCurrentProcess().ProcessName + " " +
                    Convert.ToInt32(_splashWindow.Left) + " " +
                    Convert.ToInt32(_splashWindow.Top);
                });

                System.Console.WriteLine(arguments);

                // запуск UpgradeTool.exe
                Process proc = new Process();
                proc.StartInfo.FileName = gamePath + "/UpgradeTool.exe";
                proc.StartInfo.Arguments = arguments;
                proc.Start();
            }
            //catch
            //{
            //    MessageBox.Show("Не удалось обновить лаунчер!");
            //}
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
            System.Console.WriteLine("DLL LOAD " + string.Join(", ", args.Name));
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

            if (args.Name.Contains("System.IO.Compression"))
            {
                return Assembly.Load(Resources.Compression);
            }

            return null;
        }

        /// <summary>
        /// Иницализация стилей приложения.
        /// </summary>
        private static void StylesInit()
        {
            const string resources = "pack://application:,,,/Gui/Resources/";
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Assets/langs/" + "ru-RU.xaml")
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(resources + "Fonts.xaml")
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(resources + "Colors.xaml")
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(resources + "Iconics.xaml")
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(resources + "Defaults.xaml")
            });
            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(resources + "TextBoxStyles.xaml")
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(resources + "TabControlStyles.xaml")
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(resources + "ButtonStyles.xaml")
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(resources + "ListboxStyles.xaml")
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(resources + "StylesDictionary.xaml")
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(resources + "ComboBoxStyles.xaml")
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Controls/Controls.xaml")
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/DataTemplates.xaml")
            });
        }

        private static int importantThreads = 0;
        private static ManualResetEvent waitingClosing = new ManualResetEvent(true);
        private static object locker = new object();

        /// <summary>
        /// Добавляет приоритетную задачу. При выключении лаунчер будет ждать завершения всех приоритетных задач.
        /// </summary>
        public static void AddImportantTask()
        {
            lock (locker)
            {
                importantThreads++;
                waitingClosing.Reset();
            }
        }

        /// <summary>
        /// Сообщает что приоритетная задача выполнена.
        /// </summary>
        public static void RemoveImportantTask()
        {
            lock (locker)
            {
                importantThreads--;
                if (importantThreads == 0)
                {
                    waitingClosing.Set();
                }
            }
        }

        private static bool _exitIsCanceled = false;
        private static bool _inExited = false;
        private static TaskbarIcon nofityIcon;

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

            BeforeExit(null, null);
            Environment.Exit(0);
        }

        public static void BeforeExit(object sender, EventArgs e)
        {
            //закрываем все окна
            foreach (Window window in app.Windows)
            {
                window.Close();
            }

            ExitEvent?.Invoke();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TaskRun(ThreadStart threadFunc) => new Thread(threadFunc).Start();

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

            app.Dispatcher.Invoke(() => {
                if (app.MainWindow == null) 
                { 
                    app.MainWindow = new MainWindow() 
                    {
                        Left = app.MainWindow.Left - 322,
                        Top = app.MainWindow.Top - 89
                    };
                    app.MainWindow.Show();
                }
            });
        }

        public static void CloseMainWindow()
        {
            System.Console.WriteLine("Test");
            app.Dispatcher.Invoke(() =>
            {
                if (app.MainWindow != null) 
                { 
                    app.MainWindow.Close();
                    app.MainWindow = null;
                }
            });
        }
    }
}

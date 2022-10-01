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
        private static Dictionary<string, Lexplosion.Gui.Views.Windows.Console> ConsoleList = new Dictionary<string, Lexplosion.Gui.Views.Windows.Console>();

        public static Process CurrentProcess { get; private set; }

        public static event Action ExitEvent;


        [STAThread]
        static void Main()
        {
            app.Exit += BeforeExit;

            Thread thread = new Thread(InitializedSystem);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            _splashWindow = new SplashWindow();
            _splashWindow.ChangeLoadingBoardPlaceholder();
            app.Run(_splashWindow);
        }

        private static void InitializedSystem()
        {
            //System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>> headers = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>>();
            //headers.Add(new System.Collections.Generic.KeyValuePair<string, string>("x-api-key", "$2a$10$Ky9zG9R9.ha.kf5BRrvwU..OGSvC0I2Wp56hgXI/4aRtGbizrm3we"));
            //string answer = ToServer.HttpGet("https://api.curseforge.com/v1/categories?gameId=432&classId=6", headers);
            //Console.WriteLine(answer);

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
                // если в настрйоках устанавлено что нужно скрывать лаунчер при запуске клиента, то скрывеам галвное окно
                if (UserData.GeneralSettings.HiddenMode == true)
                {
                    app.Dispatcher.Invoke(delegate ()
                    {
                        foreach (Window window in app.Windows)
                        {
                            if (window is Gui.Views.Windows.MainWindow)
                            {
                                window.Visibility = Visibility.Collapsed;
                                window.ShowInTaskbar = false;
                                break;
                            }
                        }
                    });
                }
            };

            // подписываемся на запуск игры до запуска окна
            LaunchGame.GameStartEvent += (string str) =>
            {
                app.Dispatcher.Invoke(() => 
                { 
                    ConsoleList.Add(str, new Gui.Views.Windows.Console()
                    {
                        Left = app.MainWindow.Left - 322,
                        Top = app.MainWindow.Top - 89
                    });
                
                    ConsoleList[str].Show();
                });
            };

            LaunchGame.GameStopEvent += delegate (string str) //подписываемся на эвент завершения игры
            {
                // если в настрйоках устанавлено что нужно скрывать лаунчер при запуске клиента, то показываем главное окно
                if (UserData.GeneralSettings.HiddenMode == true) app.Dispatcher.Invoke(MakeVisible);

                app.Dispatcher.Invoke(() =>
                {
                    ConsoleList[str].Close();
                    ConsoleList.Remove(str);
                });
            };

            Thread.Sleep(800);

            app.Dispatcher.Invoke(() =>
            {
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
            System.Console.WriteLine("UnzipBytesArray");
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

        /// <summary>
        /// Длеает все окна лаунчера видимыми и выводит их на экран
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MakeHidden()
        {
            foreach (Window window in app.Windows)
            {
                window.Visibility = Visibility.Collapsed;
                window.ShowInTaskbar = false;
            }
        }

        /// <summary>
        /// Длеает все окна лаунчера видимыми и выводит их на экран
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MakeVisible()
        {
            foreach (Window window in app.Windows)
            {
                window.Visibility = Visibility.Visible;
                window.ShowInTaskbar = true;
            }

            NativeMethods.ShowProcessWindows(Runtime.CurrentProcess.MainWindowHandle);
        }

        /// <summary>
        /// Открывает лаунчер и отменяет закрытие, если оно было.
        /// </summary>
        public static void ShowApp()
        {
            lock (locker)
            {
                if (_inExited)
                {
                    _exitIsCanceled = true;
                    waitingClosing.Set();
                }  
            }

            MakeVisible();
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
        /// Выход из лаунчера. Если запущен приоритетный процесс, то ждет его завршения и только потом закрывеат лаунчер. Закртие может быть омтенено методов ShowApp
        /// </summary>
        public static void Exit()
        {
            lock (locker)
            {
                _inExited = true;

                if (importantThreads > 0)
                {
                    foreach (Window window in app.Windows)
                    {
                        window.Visibility = Visibility.Collapsed;
                        window.ShowInTaskbar = false;
                    }
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
    }
}

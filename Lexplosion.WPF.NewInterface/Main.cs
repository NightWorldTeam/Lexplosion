using DiscordRPC;
using Lexplosion.Global;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Tools;
using Lexplosion.WPF.NewInterface.Core.Notifications;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Core.Services;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.Content.GeneralSettings;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.InstanceProfile.Settings;
using Lexplosion.WPF.NewInterface.Mvvm.Views.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Lexplosion.WPF.NewInterface
{
    public enum HeaderState
    {
        Left,
        Right,
        Top,
    }

    /*    public class NotificationManager : INotificationManager
        {
            public void Show(INotification notifiable)
            {
                RuntimeApp.Notification.Add(notifiable);
            }
        }*/

    internal static class RuntimeApp
    {
        internal const string ResourcePath = "pack://application:,,,/Resources/";
        internal const string AssetsPath = "pack://application:,,,/Assets/";
        internal const string ControlsPath = "pack://application:,,,/Controls/";

        internal static event Action MainWindowShowed;

        private static event Action ResourceDictionariesLoaded;

        private static App _app = new App();

        private static double _leftPos;
        private static double _topPos;

        private static double _splashWindowLeft;
        private static double _splashWindowTop;

        internal static AppColorThemeService AppColorThemeService;

        public static HeaderState HeaderState;

        internal static string[] ResourceNames;

        public static ICollection<INotification> Notification = new ObservableCollection<INotification>();

        [STAThread]
        static void Main()
        {
            //SetupTestEnviroment();
            //return;
            AppColorThemeService = new AppColorThemeService();
            //var title = "TKESKLTSRLK ALLALA";
            //var message = "Действие фильма будет происходить после событий, рассказанных в фильме «Миссия невыполнима: Последствия». В центре истории новые приключения агента Итана Ханта.";

            //Notification.Add(new InstanceNotification(title, message, NotificationType.Info, TimeSpan.MaxValue));
            // Подписываемся на эвент для загрузки всех строенных dll'ников
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

            //app.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            //_splashWindow = new SplashWindow();
            //_splashWindow.ChangeLoadingBoardPlaceholder();

            //_splashWindowLeft = _splashWindow.Left;
            //_splashWindowTop = _splashWindow.Top;

            ResourceDictionariesLoaded += SetMainWindow;

            _app.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            InitializedSystem();

            //ResourcesDictionariesRegister();
            //SetMainWindow();

            //Thread thread = new Thread(InitializedSystem);
            // thread.SetApartmentState(ApartmentState.STA);
            // thread.Start();

            //app.Run(_splashWindow);
        }

        private static void SetupTestEnviroment()
        {
            _app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(ControlsPath + "Controls.xaml")
            });

            _app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(ResourcePath + "ResourcesRegister.xaml")
            });

            _app.MainWindow = new TestWindow();
            _app.MainWindow.Show();
            _app.Run(_app.MainWindow);
        }

        public static string[] GetResourceNames()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resName = assembly.GetName().Name + ".g.resources";
            using (var stream = assembly.GetManifestResourceStream(resName))
            {
                using (var reader = new System.Resources.ResourceReader(stream))
                {
                    return reader.Cast<DictionaryEntry>().Select(entry =>
                             (string)entry.Key).ToArray();
                }
            }
        }

        private static void SetMainWindow()
        {
            ResourceNames = GetResourceNames();

            //_nofityIcon = (TaskbarIcon)App.Current.FindResource("NofityIcon");

            var mainWindow = new MainWindow()
            {
                Left = App.Current.MainWindow.Left - 322,
                Top = App.Current.MainWindow.Top - 89
            };

            _leftPos = mainWindow.Left;
            _topPos = mainWindow.Top;

            mainWindow.Show();
            //(App.Current.MainWindow as SplashWindow).SmoothClosing();
            App.Current.MainWindow = mainWindow;
            App.Current.Run(mainWindow);
            /*
                        _app.MainWindow = new MainWindow();//new TestWindow();
                        //_app.MainWindow = new TestWindow();
                        _app.MainWindow.Show();
                        _app.Run(_app.MainWindow);*/
        }

        private static void InitializedSystem()
        {
            Runtime.InitializedSystem((int)0, (int)0, false);

            ResourcesDictionariesRegister();

            InitializedAccountSystem();

            App.Current.Dispatcher.Invoke(delegate ()
            {
                App.Current.Exit += Runtime.BeforeExit;
            });

            Runtime.ПереходВРежимЗавершения += CloseMainWindow;
            Runtime.OnExitEvent += ExitHandler;
            Runtime.OnUpdateStart += () => App.Current.Dispatcher.Invoke(() =>
            {
                //_splashWindow.ChangeLoadingBoardPlaceholder(true);
            });

            Runtime.OnLexplosionOpened += ShowMainWindow;

            Runtime.InitializedSystem((int)_splashWindowLeft, (int)_splashWindowTop);

            // Выставляем язык
            LoadCurrentLanguage();
            // Встраеваем стили
            //App.Current.Dispatcher.Invoke(StylesInit);

            // инициализация окна уведомлений
            // InitializeNotificationWindow();

            var discordClient = InitDiscordApp();

            LaunchGame _activeGameManager = null;

            LaunchGame.OnGameProcessStarted += (LaunchGame gameManager) =>
            {
                _activeGameManager = gameManager;

                if (gameManager.ClientSettings.IsShowConsole == true)
                {
                    //App.Current.Dispatcher.Invoke(() => ConsoleWindow.SetWindow(gameManager));
                }
            };

            LaunchGame.OnGameStarted += delegate (LaunchGame gameManager) //подписываемся на эвент запуска игры
            {
                // если в настрйоках устанавлено что нужно скрывать лаунчер при запуске клиента, то скрывеам главное окно
                if (GlobalData.GeneralSettings.IsHiddenMode == true)
                {
                    CloseMainWindow();
                }

                discordClient?.SetPresence(new RichPresence()
                {
                    State = "Minecraft " + gameManager.GameVersion,
                    Details = "Сборка - " + gameManager.GameClientName,
                    Timestamps = Timestamps.Now,
                    Assets = new Assets()
                    {
                        LargeImageKey = "logo1"
                    }
                });
            };

            LaunchGame.OnGameStoped += delegate (LaunchGame gameManager) //подписываемся на эвент завершения игры
            {
                _activeGameManager = null;

                // если в настрйоках устанавлено что нужно скрывать лаунчер при запуске клиента, то показываем главное окно
                if (gameManager.ClientSettings.IsHiddenMode == true)
                {
                    ShowMainWindow();
                }

                discordClient?.SetPresence(new RichPresence()
                {
                    State = "Minercaft не запущен.",
                    Timestamps = Timestamps.Now,
                    Assets = new Assets()
                    {
                        LargeImageKey = "logo1"
                    }
                });
            };

            // Если в глобальных настройках включена консоль.
            GeneralSettingsModel.ConsoleParameterChanged += delegate (bool isShow)
            {
                if (isShow && _activeGameManager != null)
                {
                    //App.Current.Dispatcher.Invoke(() => ConsoleWindow.SetWindow(_activeGameManager));
                }
            };

            // Если в настройках сборки, включена консоль.
            InstanceProfileSettingsModel.ConsoleParameterChanged += delegate (bool isShow, string instanceId)
            {
                if (isShow && _activeGameManager != null && _activeGameManager.InstanceId == instanceId)
                {
                    //App.Current.Dispatcher.Invoke(() => ConsoleWindow.SetWindow(_activeGameManager));
                }
            };

            Thread.Sleep(800);

            _app.Dispatcher.Invoke(SetMainWindow);
        }

        private static void InitializedAccountSystem()
        {
            Runtime.TaskRun(() => {
                var latestActiveAccount = Account.ActiveAccount;
                if (latestActiveAccount != null)
                {
                    var code = latestActiveAccount.Auth();
                    if (code != AuthCode.Successfully)
                    {

                    }
                }
            });
        }

        private static DiscordRpcClient InitDiscordApp()
        {
            DiscordRpcClient client = new DiscordRpcClient(LaunсherSettings.DiscordAppID);

            if (client.Initialize())
            {
                return null;
            }

            client.SetPresence(new RichPresence()
            {
                State = "Minecraft не запущен",
                Timestamps = Timestamps.Now,
                Assets = new Assets()
                {
                    LargeImageKey = "logo1"
                }
            });

            return client;
        }

        public static void ExitHandler()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (Window window in App.Current.Windows)
                {
                    window.Close();
                }

                //_nofityIcon.Dispose();
            });
        }

        public static void ShowMainWindow()
        {
            Runtime.CancelingExit();
            App.Current.Dispatcher.Invoke(() =>
            {
                if (App.Current.MainWindow == null)
                {
                    NativeMethods.ShowProcessWindows(Runtime.CurrentProcess.MainWindowHandle);
                    return;
                }

                App.Current.MainWindow = new MainWindow()
                {
                    Left = _leftPos,
                    Top = _topPos
                };
                App.Current.MainWindow.Show();
            });
        }

        public static void CloseMainWindow()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (App.Current.MainWindow != null)
                {
                    try
                    {
                        _leftPos = App.Current.MainWindow.Left;
                        _topPos = App.Current.MainWindow.Top;
                    }
                    catch { }

                    try
                    {
                        App.Current.MainWindow.Close();
                    }
                    catch { }

                    try
                    {
                        App.Current.MainWindow = null;
                    }
                    catch { }
                }
            });
        }



        public static void LoadCurrentLanguage()
        {
            var selectedLangId = GlobalData.GeneralSettings.LanguageId;
            var currentCultureName = Thread.CurrentThread.CurrentCulture.ToString();

            if (selectedLangId.Length == 0)
            {

            }
            else
            {

            }

            Runtime.DebugWrite(currentCultureName);

            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Assets/langs/" + GlobalData.GeneralSettings.LanguageId + ".xaml")
            });
        }

        private static void ResourcesDictionariesRegister()
        {
            // Languages //
            _app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(AssetsPath + "LanguagesRegister.xaml")
            });
            // Themes //
            _app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(ResourcePath + "ThemesRegistry.xaml")
            });
            // TODO: Избавить Control от зависимости от цветов тем.
            // Controls //
            _app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(ControlsPath + "Controls.xaml")
            });
            // Resources //
            _app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(ResourcePath + "ResourcesRegister.xaml")
            });
            // DataTemplates //
            _app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/DataTemplates.xaml")
            });
        }

        private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            //if (args.Name.Contains("Lexplosion.Core"))
            //{
            //    return Assembly.Load(UnzipBytesArray(Resources.LexplosionCore));
            //}

            //if (args.Name.Contains("Newtonsoft.Json"))
            //{
            //    return Assembly.Load(UnzipBytesArray(Resources.NewtonsoftJson));
            //}

            //if (args.Name.Contains("LumiSoft.Net"))
            //{
            //    return Assembly.Load(UnzipBytesArray(Resources.LumiSoft_Net));
            //}

            //if (args.Name.Contains("Tommy"))
            //{
            //    return Assembly.Load(UnzipBytesArray(Resources.Tommy));
            //}

            //if (args.Name.Contains("Hardcodet.Wpf.TaskbarNotification"))
            //{
            //    return Assembly.Load(UnzipBytesArray(Resources.TaskbarNotification));
            //}

            //if (args.Name.Contains("DiscordRPC"))
            //{
            //    return Assembly.Load(UnzipBytesArray(Resources.DiscordRPC));
            //}

            //if (args.Name.Contains("System.IO.Compression"))
            //{
            //    return Assembly.Load(Resources.Compression);
            //}

            return null;
        }
    }
}

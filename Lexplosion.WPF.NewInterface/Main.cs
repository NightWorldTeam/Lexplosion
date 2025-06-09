using DiscordRPC;
using Hardcodet.Wpf.TaskbarNotification;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Management.Instances;

using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Notifications;
using Lexplosion.WPF.NewInterface.Core.Tools;
using Lexplosion.WPF.NewInterface.Extensions;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.Content.GeneralSettings;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.InstanceProfile.Settings;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Authorization;
using Lexplosion.WPF.NewInterface.Mvvm.Views.Windows;
using Lexplosion.WPF.NewInterface.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface
{
    public enum HeaderState
    {
        Left,
        Right,
        Top,
    }

    internal static class RuntimeApp
    {
        internal const string ResourcePath = "pack://application:,,,/Resources/";
        internal const string AssetsPath = "pack://application:,,,/Assets/";
        internal const string ControlsPath = "pack://application:,,,/Controls/";

        internal static event Action MainWindowShowed;

        private static App _app = new App();

        private static double _leftPos;
        private static double _topPos;

        private static SplashWindow _splashWindow;
        private static double _splashWindowLeft;
        private static double _splashWindowTop;
        private static TaskbarIcon _nofityIcon;

        public static HeaderState HeaderState;
        public static event Action TrayMenuElementClicked;
        public static event Action TrayContextMenuOpened;

        internal static AppCore _appCore;
        internal static string[] ResourceNames;

        public static ICollection<INotification> Notification = new ObservableCollection<INotification>();

        [STAThread]
        static void Main()
        {
            //SetupTestEnviroment();
            //return;
            // Подписываемся на эвент для загрузки всех строенных dll'ников
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

            _appCore = new AppCore(App.Current.Dispatcher.Invoke, (key) => App.Current.Resources[key], RestartApp);

            _app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(ResourcePath + "ThemesRegistry.xaml")
            });


            App.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            _splashWindow = new SplashWindow();
            _splashWindow.ChangeLoadingBoardPlaceholder();

            _splashWindowLeft = _splashWindow.Left;
            _splashWindowTop = _splashWindow.Top;

            _app.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            Thread thread = new Thread(InitializedSystem);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            App.Current.Run(_splashWindow);
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

        public static void ChangeToolTipState(bool state)
        {
            Style style = (App.Current.Resources[state ? "DefaulToolTip" : "HiddenToolTip"] as Style);
            App.Current.Resources[typeof(ToolTip)] = style;
        }

        public static void ChangeSettingInitialShowDelay(int value)
        {
            App.Current.Resources["SettingInitialShowDelay"] = value;
        }

        public static void ChangeSettingBetweenShowDelay(int value)
        {
            App.Current.Resources["SettingBetweenShowDelay"] = value;
        }

        private static void SetMainWindow(bool firstLaunch = false)
        {
            ResourceNames = GetResourceNames();

            App.Current.MainWindow.Topmost = true;

            // инициализируем mainViewModel.
            MainViewModel mainViewModel;
            if (!App.Current.Resources.TryGetValue("MainViewModel", out mainViewModel))
            {
                mainViewModel = new MainViewModel(_appCore);
                App.Current.Resources["MainViewModel"] = mainViewModel;
            }


            ViewModelBase viewmodel = mainViewModel;

            if (firstLaunch)
            {
                bool newUser = Runtime.IsFirtsLaunch;
                if (newUser)
                {
                    viewmodel = GetWelcomeViewModel(() => NavigateAfterWelcomePage(mainViewModel.ToMainMenu));
                }
                else
                {
                    NavigateAfterWelcomePage(mainViewModel.ToMainMenu);
                }
            }

            var mainWindow = new MainWindow(_appCore)
            {
                Left = App.Current.MainWindow.Left - 322,
                Top = App.Current.MainWindow.Top - 89,
                DataContext = viewmodel,
            };

            if (firstLaunch)
            {
                Account.AccountDeleted += (account) =>
                {
                    if (Account.ListCount == 0)
                    {
                        mainWindow.DataContext = null;
                        mainWindow.DataContext = GetAuthorizationViewModel(mainViewModel.ToMainMenu);
                    }
                };
            }

            _leftPos = mainWindow.Left;
            _topPos = mainWindow.Top;

            (App.Current.MainWindow as SplashWindow).SmoothClosing();
            mainWindow.Show();
            App.Current.MainWindow = mainWindow;

        }

        private static void NavigateAfterWelcomePage(ICommand toMainMenu)
        {
            if (Account.List.Count() == 0)
            {
                GetAuthorizationViewModel(toMainMenu);
            }
            else
            {
                toMainMenu.Execute(null);
            }
        }

        private static ViewModelBase GetAuthorizationViewModel(ICommand toMainMenu)
        {
            ViewModelBase viewmodel = new AuthorizationMenuViewModel(_appCore, toMainMenu);
            _appCore.NavigationStore.CurrentViewModel = viewmodel;
            return viewmodel;
        }

        private static ViewModelBase GetWelcomeViewModel(Action navigate)
        {
            ViewModelBase welcomePageViewmodel = new WelcomeViewModel(_appCore, navigate);
            _appCore.NavigationStore.CurrentViewModel = welcomePageViewmodel;
            return welcomePageViewmodel;
        }

        private static void InitializedSystem()
        {
			ResourcesDictionariesRegister();

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

			InitializedAccountSystem();

			_appCore.UIThread.Invoke(() =>
            {
                _appCore.Settings = new AppSettings();
            });

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
                    App.Current.Dispatcher.Invoke(() => ConsoleWindow.SetWindow(gameManager));
                }
            };

            LaunchGame.OnGameStarted += (LaunchGame gameManager) => //подписываемся на эвент запуска игры
            {
                // если в настрйоках устанавлено что нужно скрывать лаунчер при запуске клиента, то скрывеам главное окно
                if (GlobalData.GeneralSettings.IsHiddenMode == true)
                {
                    CloseMainWindow();
                }

                // TODO: Translate
                discordClient?.SetPresence(new RichPresence()
                {
                    State = "Minecraft " + gameManager.GameVersion,
                    Details = string.Format((_appCore.Resources("InstanceDash_") as string), gameManager.GameClientName),
                    Timestamps = Timestamps.Now,
                    Assets = new Assets()
                    {
                        LargeImageKey = "logo1"
                    }
                });
            };

            LaunchGame.OnGameStoped += (LaunchGame gameManager) => //подписываемся на эвент завершения игры
            {
                _activeGameManager = null;

                // если в настрйоках устанавлено что нужно скрывать лаунчер при запуске клиента, то показываем главное окно
                if (gameManager.ClientSettings.IsHiddenMode == true)
                {
                    ShowMainWindow();
                }

                discordClient?.SetPresence(new RichPresence()
                {
                    State = _appCore.Resources("MinecraftNotRunning") as string,
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
                    App.Current.Dispatcher.Invoke(() => ConsoleWindow.SetWindow(_activeGameManager));
                }
            };

            // Если в настройках сборки, включена консоль.
            InstanceProfileSettingsModel.ConsoleParameterChanged += delegate (bool isShow, string instanceId)
            {
                if (isShow && _activeGameManager != null && _activeGameManager.InstanceId == instanceId)
                {
                    App.Current.Dispatcher.Invoke(() => ConsoleWindow.SetWindow(_activeGameManager));
                }
            };

			InstanceClient.LogoGenerator = ImageTools.GenerateRandomIcon;


			Thread.Sleep(800);

            _appCore.UIThread(() =>
            {
                App.Current.Resources["MainViewModel"] = new MainViewModel(_appCore);

                // Загружаем Интерфейс для tray
                _app.Resources.MergedDictionaries.Add(new ResourceDictionary()
                {
                    Source = new Uri(ResourcePath + "TrayView.xaml")
                });

                _nofityIcon = (TaskbarIcon)App.Current.FindResource("NofityIcon");
                _nofityIcon.TrayContextMenuOpen += _nofityIcon_TrayContextMenuOpen;

                SetMainWindow(true);
            });
        }

        private static void _nofityIcon_TrayContextMenuOpen(object sender, RoutedEventArgs e)
        {
            // Обновляем элементы меню, так как ContextMenu находится в другом визуальном дереве
            // и через DynamicResource обновление просто не сделать
            TrayContextMenuOpened?.Invoke();
        }

        private static void InitializedAccountSystem()
        {
            Runtime.TaskRun(() =>
            {
                var latestActiveAccount = Account.ActiveAccount;
                if (latestActiveAccount != null)
                {
                    var code = latestActiveAccount.Auth();
                    if (code != AuthCode.Successfully)
                    {
                        // TODO: тебе тут явно че-то делать надо
                    }
                    else
                    {
                        Account.SaveAll();
                    }
                }
            });
        }

        private static DiscordRpcClient InitDiscordApp()
        {
            DiscordRpcClient client = new DiscordRpcClient(LaunсherSettings.DiscordAppID);

			if (!client.Initialize())
			{
				return null;
			}

			client.SetPresence(new RichPresence()
            {
                State = _appCore.Resources("MinecraftNotRunning") as string,
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

                _nofityIcon.Dispose();
            });
        }

        public static void ShowMainWindow()
        {
            Runtime.CancelingExit();
            App.Current.Dispatcher.Invoke(() =>
            {
                if (App.Current.MainWindow == null)
                {
                    App.Current.MainWindow = new MainWindow(_appCore)
                    {
                        Left = _leftPos,
                        Top = _topPos
                    };
                    App.Current.MainWindow.Show();
                }
                else
                {
					Lexplosion.Tools.NativeMethods.ShowProcessWindows(Runtime.CurrentProcess.MainWindowHandle);
                }
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
                        App.Current.MainWindow = null;
                    }
                    catch { }
                }
            });
        }


        private static ResourceDictionary CurrentLangDict;
        internal const string LangPath = AssetsPath + "langs/";

        public static void LoadCurrentLanguage()
        {
            var selectedLangId = GlobalData.GeneralSettings.LanguageId;

            if (string.IsNullOrWhiteSpace(selectedLangId))
            {
                try
                {
                    if (GlobalData.GeneralSettings.LanguageId.Length == 0)
                    {
                        var currentCultureName = Thread.CurrentThread.CurrentCulture.ToString();
                        //switch () тут код для стран cis
                        CurrentLangDict.Source = new Uri(LangPath + currentCultureName + ".xaml");
                        GlobalData.GeneralSettings.LanguageId = currentCultureName;
						Runtime.ServicesContainer.DataFilesService.SaveSettings(GlobalData.GeneralSettings);
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
					Runtime.ServicesContainer.DataFilesService.SaveSettings(GlobalData.GeneralSettings);
                }
            }
            else
            {

            }

            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Assets/langs/" + selectedLangId + ".xaml")
            });
        }

        private static void ResourcesDictionariesRegister()
        {
            // Languages //
            _app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(AssetsPath + "LanguagesRegister.xaml")
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

        public static void TrayMenuElementClickExecute()
        {
            TrayMenuElementClicked?.Invoke();
        }

        private static void RestartApp()
        {
            try
            {
                Process.Start(Application.ResourceAssembly.Location);
                Runtime.KillApp();
            }
            catch (Exception e)
            {
                throw new Exception("Restart Launcher Error. " + e);
            }
        }

        private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("Lexplosion.Core"))
            {
                return Assembly.Load(UnzipBytesArray(Resources.LexplosionCore));
            }

            if (args.Name.Contains("Newtonsoft.Json"))
            {
                return Assembly.Load(UnzipBytesArray(Resources.NewtonsoftJson));
            }

            if (args.Name.Contains("LumiSoft.Net"))
            {
                return Assembly.Load(UnzipBytesArray(Resources.LumiSoftNet));
            }

            if (args.Name.Contains("Tommy"))
            {
                return Assembly.Load(UnzipBytesArray(Resources.Tommy));
            }

            if (args.Name.Contains("Hardcodet.Wpf.TaskbarNotification"))
            {
                return Assembly.Load(UnzipBytesArray(Resources.TaskbarNotification));
            }

            if (args.Name.Contains("DiscordRPC"))
            {
                return Assembly.Load(UnzipBytesArray(Resources.DiscordRPC));
            }

            if (args.Name.Contains("VirtualizingWrapPanel"))
            {
                return Assembly.Load(UnzipBytesArray(Resources.VirtualizingWrapPanel));
            }

            if (args.Name.Contains("System.IO.Compression"))
            {
                return Assembly.Load(Resources.Compression);
            }

            return null;
        }

        private static byte[] UnzipBytesArray(byte[] zipBytes)
        {
            // TODO: Использовать MemeryStream?
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

    }
}

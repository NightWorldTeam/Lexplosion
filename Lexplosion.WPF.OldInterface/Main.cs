using DiscordRPC;
using Hardcodet.Wpf.TaskbarNotification;
using Lexplosion.Common.Models;
using Lexplosion.Common.Models.ShowCaseMenu;
using Lexplosion.Common.Views.Windows;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management;
using Lexplosion.Properties;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using ColorConverter = System.Windows.Media.ColorConverter;
using ConsoleWindow = Lexplosion.Common.Views.Windows.Console;

/*
 * Лаунчер Lexplosion. Разработано NightWorld Team.
 * Никакие права не защищены.
 * Главный исполняемый файл лаунчера. Здесь людей ебут
 */

namespace Lexplosion
{
    static class RuntimeApp
    {
        public static readonly string[] AvaliableLanguages = new string[]
        {
            "ru-RU", "en-US"
        };

        const string AssetsPath = "pack://application:,,,/Assets/";
        const string ResourcePath = "pack://application:,,,/Common/Resources/";
        internal const string LangPath = AssetsPath + "langs/";

        private static App app = new App();
        private static SplashWindow _splashWindow;
        private static NotificationWindow _notificationWindow;
        private static TaskbarIcon _nofityIcon;

        public static event Action TrayMenuElementClicked;

        // TODO: сохранять координаты закрытого главного окна, использовать при открытии следующего

        private static double leftPos;
        private static double topPos;

        private static double _splashWindowLeft;
        private static double _splashWindowTop;

        private static ResourceDictionary CurrentLangDict;

        public static Color CurrentAccentColor => (Color)app.Resources["ActivityColor"];
        public static Color[] AccentColors;

        [STAThread]
        static void Main()
        {
            // Подписываемся на эвент для загрузки всех строенных dll'ников
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            _splashWindow = new SplashWindow();
            _splashWindow.ChangeLoadingBoardPlaceholder();

            _splashWindowLeft = _splashWindow.Left;
            _splashWindowTop = _splashWindow.Top;

            Thread thread = new Thread(InitializedSystem);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            app.Run(_splashWindow);
        }


        #region Notification Window


        private static void InitializeNotificationWindow()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _notificationWindow = new NotificationWindow();
                var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
                _notificationWindow.Left = desktopWorkingArea.Right - _notificationWindow.MaxWidth;
                _notificationWindow.Top = desktopWorkingArea.Bottom - 20;
            });
        }

        internal static void OpenNotificationWindow()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _notificationWindow.Show();
            });
        }

        internal static void CloseNotificationWindow()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _notificationWindow.Hide();
            });
        }


        #endregion NotificationWindow


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

        private static void InitializedSystem()
        {
            app.Resources["BrandSolidColorBrush"] = new SolidColorBrush(Color.FromRgb(22, 127, 252));
            MainStylesInit();

            app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/DataTemplates.xaml")
            });

            app.Dispatcher.Invoke(delegate ()
            {
                app.Exit += Runtime.BeforeExit;
            });

            Runtime.ПереходВРежимЗавершения += CloseMainWindow;
            Runtime.OnExitEvent += ExitHandler;
            Runtime.OnUpdateStart += () =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    _splashWindow.ChangeLoadingBoardPlaceholder(true);
                });
            };
            Runtime.OnLexplosionOpened += ShowMainWindow;

            Runtime.InitializedSystem((int)_splashWindowLeft, (int)_splashWindowTop);

            // Выставляем язык
            ChangeCurrentLanguage();
            // Встраеваем стили
            app.Dispatcher.Invoke(StylesInit);

            // инициализация окна уведомлений
            InitializeNotificationWindow();

            var discordClient = InitDiscordApp();

            LaunchGame _activeGameManager = null;

            LaunchGame.OnGameProcessStarted += (LaunchGame gameManager) =>
            {
                _activeGameManager = gameManager;

                if (gameManager.ClientSettings.IsShowConsole == true)
                {
                    app.Dispatcher.Invoke(() => ConsoleWindow.SetWindow(gameManager));
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
                    State = ResourceGetter.GetString("minecraft") + " " + gameManager.GameVersion,
                    Details = ResourceGetter.GetString("instance") + " " + gameManager.GameClientName,
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
                    State = ResourceGetter.GetString("minecraftIsNotRunning"),
                    Timestamps = Timestamps.Now,
                    Assets = new Assets()
                    {
                        LargeImageKey = "logo1"
                    }
                });
            };

            GeneralSettingsModel.ConsoleParameterChanged += delegate (bool isShow)
            {
                if (isShow && _activeGameManager != null)
                {
                    app.Dispatcher.Invoke(() => ConsoleWindow.SetWindow(_activeGameManager));
                }
            };

            InstanceSettingsModel.ConsoleParameterChanged += delegate (bool isShow, string instanceId)
            {
                if (isShow && _activeGameManager != null && _activeGameManager.InstanceId == instanceId)
                {
                    app.Dispatcher.Invoke(() => ConsoleWindow.SetWindow(_activeGameManager));
                }
            };
            Thread.Sleep(800);

            app.Dispatcher.Invoke(SetMainWindow);
            _splashWindow = null;
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
                return Assembly.Load(UnzipBytesArray(Resources.LumiSoft_Net));
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
                Runtime.KillApp();
            }
        }

        private static void MainStylesInit()
        {
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
                    State = ResourceGetter.GetString("minecraftIsNotRunning"),
                    Timestamps = Timestamps.Now,
                    Assets = new Assets()
                    {
                        LargeImageKey = "logo1"
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

        public static void ExitHandler()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (Window window in app.Windows)
                {
                    window.Close();
                }

                _nofityIcon.Dispose();
            });
        }

        public static void ShowMainWindow()
        {
            Runtime.CancelingExit();
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
                    NativeMethods.ShowProcessWindows(Runtime.CurrentProcess.MainWindowHandle);
                }
            });
        }

        public static void CloseMainWindow()
        {
            app.Dispatcher.Invoke(() =>
            {
                if (app.MainWindow != null)
                {
                    try
                    {
                        leftPos = app.MainWindow.Left;
                        topPos = app.MainWindow.Top;
                    }
                    catch { }

                    try
                    {
                        app.MainWindow.Close();
                    }
                    catch { }

                    try
                    {
                        app.MainWindow = null;
                    }
                    catch { }
                }
            });
        }
    }
}

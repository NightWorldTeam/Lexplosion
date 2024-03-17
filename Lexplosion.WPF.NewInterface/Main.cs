using Lexplosion.Core.Tools.Notification;
using Lexplosion.Global;
using Lexplosion.Logic;
using Lexplosion.Logic.Management.Authentication;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Views.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace Lexplosion.WPF.NewInterface
{
    public enum HeaderState
    {
        Left,
        Right,
        Top,
    }

    public class NotificationManager : INotificationManager
    {
        public void Show(INotificable notifiable)
        {
            RuntimeApp.Notification.Add(notifiable);
        }
    }

    internal static class RuntimeApp
    {
        internal const string ResourcePath = "pack://application:,,,/Resources/";
        internal const string AssetsPath = "pack://application:,,,/Assets/";
        internal const string ControlsPath = "pack://application:,,,/Controls/";

        private static event Action ResourceDictionariesLoaded;

        private static App _app = new App();

        private static double _leftPos;
        private static double _topPos;

        private static double _splashWindowLeft;
        private static double _splashWindowTop;

        public static HeaderState HeaderState;

        public static ICollection<INotificable> Notification = new ObservableCollection<INotificable>();

        [STAThread]
        static void Main()
        {
            //GlobalData.SetUser(new User("Hel2x", "d66ec2c0-7a35-4e8a-a3d8-d1cb913fa71c", "", "", AccountType.NoAuth, ActivityStatus.Online));

                AuthCode authCode = Authentication.Instance.Auth(
                    AccountType.NightWorld,
                    "Editor",
                    "1",
                    true);

            var title = "TKESKLTSRLK ALLALA";
            var message = "Действие фильма будет происходить после событий, рассказанных в фильме «Миссия невыполнима: Последствия». В центре истории новые приключения агента Итана Ханта.";

            Notification.Add(new InstanceNotification(title, message, NotificationType.Info, TimeSpan.MaxValue));
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

        private static void SetMainWindow()
        {
            _app.MainWindow = new MainWindow();

            _app.MainWindow.Show();
            _app.Run(_app.MainWindow);

            Runtime.DebugWrite("SetMainWindow");
        }

        private static void InitializedSystem()
        {
            Runtime.InitializedSystem((int)0, (int)0, false);

            ResourcesDictionariesRegister();
            LoadCurrentLanguage();
            _app.Dispatcher.Invoke(SetMainWindow);
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
            // Resources //
            _app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(ResourcePath + "ResourcesRegister.xaml")
            });
            // Controls //
            _app.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(ControlsPath + "Controls.xaml")
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

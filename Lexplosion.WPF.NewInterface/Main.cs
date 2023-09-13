using Lexplosion.Global;
using Lexplosion.WPF.NewInterface.Views.Windows;
using System;
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

    static class RuntimeApp
    {
        const string ResourcePath = "pack://application:,,,/Resources/";
        const string AssetsPath = "pack://application:,,,/Assets/";
        const string ControlsPath = "pack://application:,,,/Controls/";

        private static event Action ResourceDictionariesLoaded;

        private static App _app = new App();

        private static double _leftPos;
        private static double _topPos;

        private static double _splashWindowLeft;
        private static double _splashWindowTop;

        public static HeaderState HeaderState;


        [STAThread]
        static void Main()
        {
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

            ResourcesDictionariesRegister();
            SetMainWindow();

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
        }

        private static void InitializedSystem()
        {
            Runtime.InitializedSystem((int)0, (int)0);

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

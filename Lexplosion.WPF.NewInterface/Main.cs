using Lexplosion.WPF.NewInterface.Properties;
using Lexplosion.WPF.NewInterface.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Lexplosion.WPF.NewInterface
{
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
            Runtime.DebugWrite("Testseklk;l");
           
            _app.MainWindow = new MainWindow();

            _app.MainWindow.Show();
            _app.Run(_app.MainWindow);
        }

        private static void InitializedSystem()
        {
            Runtime.InitializedSystem((int)0, (int)0);

            ResourcesDictionariesRegister();

            _app.Dispatcher.Invoke(SetMainWindow);
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

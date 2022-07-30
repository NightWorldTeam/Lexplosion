using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Threading;
using System.Runtime.InteropServices;
using Lexplosion.Properties;
using Lexplosion.Global;
using Lexplosion.Logic;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Network;
using Lexplosion.Gui.Views.Windows;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Tools;

/*
 * Лаунчер Lexplosion. Создано NightWorld Team в 2019 году.
 * Последнее обновление в феврале 2021 года
 * Главный исполняемый файл лаунчера. Здесь людей ебут
 */

namespace Lexplosion
{
    static class Run
    {
        public static StreamList threads = new StreamList();
        private static App app = new App();
        public delegate void StopTask();

        public static event Action ExitEvent;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int showWindowCommand);

        [STAThread]
        static void Main()
        {
            app.Exit += BeforeExit;

            Thread thread = new Thread(InitializedSystem);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            app.Run(new SplashWindow());
        }

        private static void InitializedSystem()
        {
            // получем процессы с таким же именем (то есть пытаемся получить уже запущенную копию лаунчера)
            Process[] procs = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            Process curenProcess = Process.GetCurrentProcess();

            // процессов больше одного. Знчит лаунечр уже запущен
            if (procs.Length > 1)
            {
                // делаем окно уже запущенного лаунечра активным
                foreach (Process proc in procs)
                {
                    if (proc.Id != curenProcess.Id)
                    {
                        ShowWindow(proc.MainWindowHandle, 1);
                        SetForegroundWindow(proc.MainWindowHandle);
                    }
                }

                curenProcess.Kill(); //стопаем процесс
            }

            // Встраивание dll в exe
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

            // инициализация
            UserData.InitSetting();
            WithDirectory.Create(UserData.GeneralSettings.GamePath);

            if (ToServer.CheckLauncherUpdates())
            {
                // TODO: при отсуствии коннекта с сервером тут лаунчер повиснет на секунд 30
                LauncherUpdate();
            }

            // TODO: При скачивании асетсов нужно будет сделать гифку, ибо это занимает время
            InstanceClient.DefineInstalledInstances();

            var stylePath = "pack://application:,,,/Gui/Resources/";

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(stylePath + "Fonts.xaml")
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(stylePath + "Colors.xaml")
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(stylePath + "Defaults.xaml")
            });
            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(stylePath + "TextBoxStyles.xaml")
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(stylePath + "TabControlStyles.xaml")
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(stylePath + "ButtonStyles.xaml")
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(stylePath + "ListboxStyles.xaml")
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(stylePath + "StylesDictionary.xaml")
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri(stylePath + "ComboBoxStyles.xaml")
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Controls/Controls.xaml")
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/DataTemplates.xaml")
            });


            //Console.WriteLine("TEST");
            ////https://api.curseforge.com/v1/mods/search?gameId=432&classId=432&index=0&sortField=1&sortOrder=desc&pageSize=10&gameVersion=1.12.2&modLoaderType=0&searchFilter=
            ////https://api.curseforge.com/v1/mods/search?gameId=432&classId=4471&sortField=1&sortOrder=desc&pageSize=10&index=0
            //WebRequest req = WebRequest.Create("https://api.curseforge.com/v1/minecraft/modloader/forge-14.23.5.2860");
            //req.Headers.Add("x-api-key", "$2a$10$d9HphjHPzYChRhMdu3gStu0DaJ5RGfgtogS1NIBG1c5sqhKSK6hBS");
            //((HttpWebRequest)req).Accept = "application/json";

            //using (WebResponse resp = req.GetResponse())
            //{
            //    using (Stream stream = resp.GetResponseStream())
            //    {
            //        using (StreamReader sr = new StreamReader(stream))
            //        {
            //            Console.WriteLine("hfgjhf");
            //            Console.WriteLine(sr.ReadToEnd());
            //        }
            //    }
            //}

            Thread.Sleep(1000);

            app.Dispatcher.Invoke(() =>
            {
                app.MainWindow.Topmost = true;

                var mainWindow = new MainWindow()
                {
                    Left = app.MainWindow.Left - 97,
                    Top = app.MainWindow.Top - 39
                };

                mainWindow.Show();              
                ((SplashWindow)app.MainWindow).SmoothClosing();
                app.MainWindow = mainWindow;
            });
        }

        private static void LauncherUpdate()
        {
            MessageBox.Show("Лаунчер нуждается в обновлении! Для продолжения нажмите 'ОК'");

            try
            {
                int upgradeToolVersion = Int32.Parse(ToServer.HttpPost("windows/upgradeToolVersion.html"));
                string gamePath = UserData.GeneralSettings.GamePath;

                // скачивание и проверка версии UpgradeTool.exe
                using (WebClient wc = new WebClient())
                {
                    if (DataFilesManager.GetUpgradeToolVersion() < upgradeToolVersion && File.Exists(gamePath + "/UpgradeTool.exe"))
                    {
                        File.Delete(gamePath + "/UpgradeTool.exe");
                        wc.DownloadFile(LaunсherSettings.URL.LauncherParts + "UpgradeTool.exe", gamePath + "/UpgradeTool.exe");
                        DataFilesManager.SetUpgradeToolVersion(upgradeToolVersion);

                    }
                    else if (!File.Exists(gamePath + "/UpgradeTool.exe"))
                    {
                        wc.DownloadFile(LaunсherSettings.URL.LauncherParts + "UpgradeTool.exe", gamePath + "/UpgradeTool.exe");
                        DataFilesManager.SetUpgradeToolVersion(upgradeToolVersion);
                    }

                }

                // запуск UpgradeTool.exe
                Process proc = new Process();
                proc.StartInfo.FileName = gamePath + "/UpgradeTool.exe";
                proc.StartInfo.Arguments = Assembly.GetExecutingAssembly().Location + " " + LaunсherSettings.URL.LauncherParts + "NightWorld.exe" + " " + Process.GetCurrentProcess().ProcessName;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.Start();
            }
            catch
            {
                MessageBox.Show("Не удалось обновить лаунчер!");
            }
        }

        private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("Newtonsoft.Json"))
            {
                return Assembly.Load(Resources.NewtonsoftJson);
            }

            if (args.Name.Contains("LumiSoft.Net"))
            {
                return Assembly.Load(Resources.LumiSoftNet);
            }

            if (args.Name.Contains("Tommy"))
            {
                return Assembly.Load(Resources.Tommy);
            }

            if (args.Name.Contains("System.IO.Compression"))
            {
                return Assembly.Load(Resources.Compression);
            }

            return null;
        }

        private static int importantThreads = 0;
        private static ManualResetEvent waitingClosing = new ManualResetEvent(true);
        private static object locker = new object();

        /// <summary>
        /// Добавляет приоритетную задачу. При выключении лаунчер будет ждать завершения всех приоритетных задач.
        /// </summary>
        public static void AddImportantTask()
        {
            importantThreads++;
            lock (locker)
            {
                waitingClosing.Reset();
            }
        }

        /// <summary>
        /// Сообщает что приоритетная задача выполнена.
        /// </summary>
        public static void RemoveImportantTask()
        {
            importantThreads--;
            lock (locker)
            {
                if (importantThreads == 0)
                {
                    waitingClosing.Set();
                }
            }
        }

        public static void Exit()
        {
            BeforeExit(null, null);
            Environment.Exit(0);
        }

        public static void BeforeExit(object sender, EventArgs e)
        {
            // TODO: сохранить все данные
            //закрываем все окна
            foreach (Window window in app.Windows)
            {
                window.Close();
            }

            // стопаем все процессы вроде скачивания и тп
            threads.StopThreads();

            waitingClosing.WaitOne(); // ждём отработки всех приоритетных задач. 
            ExitEvent?.Invoke();  
        }

        public static StopTask TaskRun(ThreadStart ThreadFunc)
        {
            threads.Wait();

            int key = threads.Add(null);

            var thread = new Thread(delegate ()
            {
                int threadKey = key;

                ThreadFunc();

                threads.RemoveAt(threadKey);
            });

            threads[key] = thread;

            thread.Start();
            threads.Release();

            return delegate ()
            {
                thread.Abort();
                int threadKey = key;
                threads.RemoveAt(threadKey);
            };
        }
    }
}

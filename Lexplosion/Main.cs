using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO.Compression;
using Lexplosion.Properties;
using Lexplosion.Global;
using Lexplosion.Tools;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Gui.Views.Windows;
using Lexplosion.Gui.ViewModels;
using Lexplosion.Logic.Network.Web;
using System.Collections.Generic;

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
            Process curentProcess = Process.GetCurrentProcess();

            // процессов больше одного. Знчит лаунечр уже запущен
            if (procs.Length > 1)
            {
                // делаем окно уже запущенного лаунечра активным
                foreach (Process proc in procs)
                {
                    if (proc.Id != curentProcess.Id)
                    {
                        ShowWindow(proc.MainWindowHandle, 1);
                        SetForegroundWindow(proc.MainWindowHandle);
                    }
                }

                curentProcess.Kill(); //стопаем процесс
            }

            // Подписываемся на эвент для загрузки всех строенных dll'ников
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

            // инициализация
            UserData.InitSetting();
            WithDirectory.Create(UserData.GeneralSettings.GamePath);

            //подписываемся на эвент вылета, чтобы логировать все необработанные исключения
            AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs args)
            {
                Exception exception = (Exception)args.ExceptionObject;
                DataFilesManager.SaveFile(LaunсherSettings.LauncherDataPath + "/crash-report_" + DateTime.Now.ToString("dd.MM.yyyy-h.mm.ss") + ".log", exception.ToString());
            };

            if (ToServer.CheckLauncherUpdates())
            {
                // TODO: при отсуствии коннекта с сервером тут лаунчер повиснет на секунд 30
                LauncherUpdate();
            }

            InstanceClient.DefineInstalledInstances();

            // Встраеваем стиили
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

            CommandReceiver.OpenModpackPage += delegate (string modpackId)
            {
                InstanceClient instance = InstanceClient.GetInstance(InstanceSource.Nightworld, modpackId);
                if (instance != null)
                {
                    Console.WriteLine("open " + modpackId);
                    MainViewModel.MainMenuVM.LogoClickCommand.Execute(instance);
                    ShowWindow(curentProcess.MainWindowHandle, 1);
                    SetForegroundWindow(curentProcess.MainWindowHandle);
                }
            };

            CommandReceiver.StartCommandServer();

            Thread.Sleep(800);

            app.Dispatcher.Invoke(() =>
            {
                app.MainWindow.Topmost = true;

                var mainWindow = new MainWindow()
                {
                    Left = app.MainWindow.Left - 322,
                    Top = app.MainWindow.Top - 89
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

        private static byte[] UnzipBytesArray(byte[] zipBytes)
        {
            Console.WriteLine("UnzipBytesArray");
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
            Console.WriteLine("DLL LOAD " + string.Join(", ", args.Name));
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

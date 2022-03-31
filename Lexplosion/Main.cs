using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using Lexplosion.Properties;
using Lexplosion.Global;
using Lexplosion.Logic;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Network;
using System.Threading;
using Lexplosion.Gui.Windows;
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

        [STAThread]
        static void Main()
        {   
            Thread thread = new Thread(new ThreadStart(InitializedSystem));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            StartSplashW();
        }

        private static void StartSplashW()
        {
            app.Run(new SplashWindow());
        }

        private static void InitializedSystem() 
        {
            // получем количество процессов с таким же именем
            int processesCount = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length;

            if (processesCount > 1)
            {
                MessageBox.Show("Лаунчер уже запущен!");
                Process.GetCurrentProcess().Kill(); //стопаем процесс
            }

            // Встраивание dll в exe
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
            //app.Exit += BeforeExit;

            // инициализация
            UserData.Settings = DataFilesManager.GetSettings();
            LaunchGame.SetDefaultSettings();
            WithDirectory.Create(UserData.Settings["gamePath"]);

            if (ToServer.CheckLauncherUpdates())
            {
                // TODO: при отсуствии коннекта с сервером тут лаунчер повиснет на секунд 30
                LauncherUpdate();
            }

            // TODO: При скачивании асетсов нужно будет сделать гифку, ибо это занимает время
            ManageLogic.DefineListInstances();
            //WithDirectory.CheckLauncherAssets();

            Application.Current.Resources = new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Gui/Styles/StylesDictionary.xaml")
            };

            OutsideDataManager.DefineInstances();

            Thread.Sleep(1000);
            app.Dispatcher.Invoke(() =>
            {
                /*var authWindow = new AuthWindow();
                app.MainWindow.Close();
                app.MainWindow = authWindow;
                app.MainWindow.Show();*/
                var authWindow = new AuthWindow();
                authWindow.Left = app.MainWindow.Left - 97;
                authWindow.Top = app.MainWindow.Top - 39;
                authWindow.Show();
                app.MainWindow.Close();
                app.MainWindow = authWindow;
            });
        }

        private static void LauncherUpdate()
        {
            MessageBox.Show("Лаунчер нуждается в обновлении! Для продолжения нажмите 'ОК'");

            try
            {
                int upgradeToolVersion = Int32.Parse(ToServer.HttpPost("windows/upgradeToolVersion.html"));

                // скачивание и проверка версии UpgradeTool.exe
                using (WebClient wc = new WebClient())
                {
                    if (DataFilesManager.GetUpgradeToolVersion() < upgradeToolVersion && File.Exists(UserData.Settings["gamePath"] + "/UpgradeTool.exe"))
                    {
                        File.Delete(UserData.Settings["gamePath"] + "/UpgradeTool.exe");
                        wc.DownloadFile(LaunсherSettings.URL.LauncherParts + "UpgradeTool.exe", UserData.Settings["gamePath"] + "/UpgradeTool.exe");
                        DataFilesManager.SetUpgradeToolVersion(upgradeToolVersion);

                    }
                    else if (!File.Exists(UserData.Settings["gamePath"] + "/UpgradeTool.exe"))
                    {
                        wc.DownloadFile(LaunсherSettings.URL.LauncherParts + "UpgradeTool.exe", UserData.Settings["gamePath"] + "/UpgradeTool.exe");
                        DataFilesManager.SetUpgradeToolVersion(upgradeToolVersion);
                    }

                }

                // запуск UpgradeTool.exe
                Process proc = new Process();
                proc.StartInfo.FileName = UserData.Settings["gamePath"] + "/UpgradeTool.exe";
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

            return null;
        }

        public static void BeforeExit(object sender, EventArgs e)
        {
            // TODO: сохранить все данные
            UserStatusSetter.Exit();
            threads.StopThreads();
        }

        public static void Exit()
        {
            BeforeExit(null, null);
            Environment.Exit(0);
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

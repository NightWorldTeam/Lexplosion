using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using Lexplosion.Properties;
using Lexplosion.Gui.Windows;
using Lexplosion.Global;
using Lexplosion.Logic;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Network;
using System.Threading;

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
        private static bool haveImportantThread = false;

        [STAThread]
        static void Main()
        {
            // получем количество процессов с таким же именем
            int processesCount = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length; 

            if(processesCount > 1)
            {
                MessageBox.Show("Лаунчер уже запущен!");
                Process.GetCurrentProcess().Kill(); //стопаем процесс
            }

            // Встраивание Newtonosoft.Json в exe
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve; 
            App app = new App();
            app.Exit += BeforeExit;

            // инициализация
            UserData.settings = DataFilesManager.GetSettings();
            LaunchGame.SetDefaultSettings();
            WithDirectory.Create(UserData.settings["gamePath"]);

            if (ToServer.CheckLauncherUpdates()) 
            {
                // TODO: при отсуствии коннекта с сервером тут лаунчер повиснет на секунд 30
                LauncherUpdate();
            }

            // TODO: При скачивании асетсов нужно будет сделать гифку, ибо это занимает время
            WithDirectory.CheckLauncherAssets();
            UserData.instancesAssets = DataFilesManager.GetLauncherAssets();

            Application.Current.Resources = new ResourceDictionary() 
            { 
                Source = new Uri("pack://application:,,,/Gui/Styles/StylesDictionary.xaml") 
            };

            app.Run(new AuthWindow());
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
                    if (DataFilesManager.GetUpgradeToolVersion() < upgradeToolVersion && File.Exists(UserData.settings["gamePath"] + "/UpgradeTool.exe"))
                    {
                        File.Delete(UserData.settings["gamePath"] + "/UpgradeTool.exe");
                        wc.DownloadFile(LaunсherSettings.serverUrl + "windows/UpgradeTool.exe", UserData.settings["gamePath"] + "/UpgradeTool.exe");
                        DataFilesManager.SetUpgradeToolVersion(upgradeToolVersion);

                    }
                    else if (!File.Exists(UserData.settings["gamePath"] + "/UpgradeTool.exe"))
                    {
                        wc.DownloadFile(LaunсherSettings.serverUrl + "windows/UpgradeTool.exe", UserData.settings["gamePath"] + "/UpgradeTool.exe");
                        DataFilesManager.SetUpgradeToolVersion(upgradeToolVersion);
                    }

                }

                // запуск UpgradeTool.exe
                Process proc = new Process();
                proc.StartInfo.FileName = UserData.settings["gamePath"] + "/UpgradeTool.exe";
                proc.StartInfo.Arguments = Assembly.GetExecutingAssembly().Location + " " + LaunсherSettings.serverUrl + "windows/NightWorld.exe" + " " + Process.GetCurrentProcess().ProcessName;
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

            return null;
        }

        public static void BeforeExit(object sender, EventArgs e)
        {
            // TODO: сохранить все данные
            if (!haveImportantThread)
            {
                threads.StopThreads();
            }

        }

        public static void Exit()
        {
            BeforeExit(null, null);
            Environment.Exit(0);
        }

        public static void ThreadRun(ThreadStart ThreadFunc, bool isImportant = false)
        {
            MessageBox.Show(threads.Count().ToString());

            haveImportantThread = haveImportantThread || isImportant;

            threads.Wait();

            var threadInfo = new StreamList.ThreadInfo
            {
                isImportant = isImportant,
                thread = null
            };

            int key = threads.Add(threadInfo);

            var thread = new Thread(delegate () 
            {
                ref StreamList threadsList = ref threads;
                int threadKey = key;

                ThreadFunc();

                threadsList.RemoveAt(threadKey);

            });

            threads[key].thread = thread;

            thread.Start();
            threads.Release();


        }
    }
}

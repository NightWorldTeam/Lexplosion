using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using Lexplosion.Gui;
using Lexplosion.Logic;
using Lexplosion.Properties;
using Lexplosion.Objects;
using System.Threading;
using Lexplosion.Gui.Windows;

/*
 * Лаунчер Lexplosion. Создано NightWorld Team в 2019 году.
 * Последнее обновление в феврале 2021 года
 * Главный исполняемый файл лаунчера
 */

namespace Lexplosion
{
    static class Run
    {
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

            // инициализация
            UserData.settings = WithDirectory.GetSettings();
            LaunchGame.SetDefaultSettings();
            WithDirectory.Create(UserData.settings["gamePath"]);

            if (ToServer.CheckLauncherUpdates()) 
            {
                // TODO: при отсуствии коннекта с сервером тут лаунчер повиснет на секунд 30
                LauncherUpdate();
            }

            new Thread(delegate () {
                // TODO: При скачивании асетсов нужно будет сделать гифку, ибо это занимает время
                WithDirectory.CheckLauncherAssets(); 
                UserData.profilesAssets = WithDirectory.GetInstanceAssets();
            }).Start();

            Application.Current.Resources = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Gui/Styles/StylesDictionary.xaml") };
            app.Run(new AuthWindow());
        }

        private static void LauncherUpdate()
        {
            MessageBox.Show("Лаунчер нуждается в обновлении! Для продолжения нажмите 'ОК'");

            try
            {
                int upgradeToolVersion = Int32.Parse(ToServer.HttpPost("windows/upgradeToolVersion.html"));

                // скачивание и проверка версии UpgradeTool.exe
                WebClient wc = new WebClient();
                if (WithDirectory.GetUpgradeToolVersion() < upgradeToolVersion && File.Exists(UserData.settings["gamePath"] + "/UpgradeTool.exe"))
                {
                    File.Delete(UserData.settings["gamePath"] + "/UpgradeTool.exe");
                    wc.DownloadFile(LaunсherSettings.serverUrl + "windows/UpgradeTool.exe", UserData.settings["gamePath"] + "/UpgradeTool.exe");
                    WithDirectory.SetUpgradeToolVersion(upgradeToolVersion);

                } else if (!File.Exists(UserData.settings["gamePath"] + "/UpgradeTool.exe")) {

                    wc.DownloadFile(LaunсherSettings.serverUrl + "windows/UpgradeTool.exe", UserData.settings["gamePath"] + "/UpgradeTool.exe");
                    WithDirectory.SetUpgradeToolVersion(upgradeToolVersion);
                }

                // запуск UpgradeTool.exe
                Process proc = new Process();
                proc.StartInfo.FileName = UserData.settings["gamePath"] + "/UpgradeTool.exe";
                proc.StartInfo.Arguments = Assembly.GetExecutingAssembly().Location + " " + LaunсherSettings.serverUrl + "windows/NightWorld.exe" + " " + Process.GetCurrentProcess().ProcessName;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.Start();

            } catch {
                MessageBox.Show("Не удалось обновить лаунчер!");
            }

        }

        private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {

            if (args.Name.Contains("Newtonsoft.Json"))
                return Assembly.Load(Resources.NewtonsoftJson);

            return null;
        }
    }
}

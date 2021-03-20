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
            int processesCount = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length; //получем количество процессов с таким же именем

            if(processesCount > 1)
            {
                MessageBox.Show("Лаунчер уже запущен!");
                Process.GetCurrentProcess().Kill(); //стопаем процесс
            }
            

            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve; //Встраивание Newtonosoft.Json в exe
            App app = new App();

            //инициализация
            UserData.settings = WithDirectory.GetSettings();
            LaunchGame.SetDefaultSettings();
            WithDirectory.Create(UserData.settings["gamePath"]);

            if (ToServer.CheckLauncherUpdates()) //при отсуствии коннекта с сервером тут лаунчер повиснет на секунд 30
            {
                LauncherUpdate();
            }

            new Thread(delegate () {
                WithDirectory.CheckLauncherAssets(); //допилить. При скачивании асетсов нужно будет сделать гифку, ибо это занимает время
                UserData.profilesAssets = WithDirectory.GetModpacksAssets();
            }).Start();

            Application.Current.Resources = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Gui/StylesDictionary.xaml") };
            app.Run(new AuthWindow());
        }

        private static void LauncherUpdate()
        {
            MessageBox.Show("Лаунчер нуждается в обновлении! Для продолжения нажмите 'ОК'");

            try
            {
                int upgradeToolVersion = Int32.Parse(ToServer.HttpPost("windows/upgradeToolVersion.html"));

                //скачивание и проверка версии UpgradeTool.exe
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

                //запуск UpgradeTool.exe
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

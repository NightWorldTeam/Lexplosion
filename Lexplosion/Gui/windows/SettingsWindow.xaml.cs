using Lexplosion.Logic;
using Lexplosion.Objects;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Lexplosion.Gui.Windows
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            MouseDown += delegate { DragMove(); };
            //getPropeties();
        }
        /*
        void getPropeties()
        {
            minMem.Text = UserData.settings["xms"];
            maxMem.Text = UserData.settings["xmx"];
            dir.Text = UserData.settings["gamePath"];
            windowWidth.Text = UserData.settings["windowWidth"];
            windowHeight.Text = UserData.settings["windowHeight"];
            JVMPath.Text = UserData.settings["javaPath"];
            JVMArgs.Text = UserData.settings["gameArgs"];

            if (UserData.settings["noUpdate"] == "true")
                UpdatesOff.IsChecked = true;

            if (UserData.settings["showConsole"] == "true")
                ShowConsole.IsChecked = true;

            if (UserData.settings["hiddenMode"] == "true")
                HiddenMode.IsChecked = true;

        }

        private void BackMainWindow(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow
            {
                Left = this.Left,
                Top = this.Top,
                WindowState = WindowState.Normal
            };

            mainWindow.Show(); mainWindow.Activate();
            this.Close();
        }


        private void GameSettings(object sender, RoutedEventArgs e)
        {
            this.gridAdvancedSettings.Visibility = Visibility.Collapsed;
            this.gridLexplosionSettings.Visibility = Visibility.Collapsed;
            this.gridGameSettings.Visibility = Visibility.Visible;
        }

        private void AdvancedSettings(object sender, RoutedEventArgs e)
        {
            this.gridGameSettings.Visibility = Visibility.Collapsed;
            this.gridLexplosionSettings.Visibility = Visibility.Collapsed;
            this.gridAdvancedSettings.Visibility = Visibility.Visible;  
        }

        private void LexplosionSettings(object sender, RoutedEventArgs e)
        {
            this.gridGameSettings.Visibility = Visibility.Collapsed;
            this.gridAdvancedSettings.Visibility = Visibility.Collapsed;
            this.gridLexplosionSettings.Visibility = Visibility.Visible;
        }

        async private void SaveSett(object sender, RoutedEventArgs e)
        {
            int num;
            if (!string.IsNullOrWhiteSpace(minMem.Text) && int.TryParse(minMem.Text, out num))
                UserData.settings["xms"] = minMem.Text;

            if (!string.IsNullOrWhiteSpace(maxMem.Text) && int.TryParse(maxMem.Text, out num))
                UserData.settings["xmx"] = maxMem.Text;

            if (!string.IsNullOrWhiteSpace(dir.Text))
                UserData.settings["gamePath"] = dir.Text.Replace(@"\", "/");

            if (!string.IsNullOrWhiteSpace(JVMPath.Text))
                UserData.settings["javaPath"] = JVMPath.Text.Replace(@"\", "/");

            if (!string.IsNullOrWhiteSpace(windowWidth.Text) && int.TryParse(windowWidth.Text, out num))
                UserData.settings["windowWidth"] = windowWidth.Text;

            if (!string.IsNullOrWhiteSpace(windowHeight.Text) && int.TryParse(windowHeight.Text, out num))
                UserData.settings["windowHeight"] = windowHeight.Text;

            if (!string.IsNullOrWhiteSpace(JVMArgs.Text))
            {
                UserData.settings["gameArgs"] = JVMArgs.Text;
                UserData.settings["gameArgs"] = UserData.settings["gameArgs"].TrimStart(' ');
                UserData.settings["gameArgs"] = UserData.settings["gameArgs"].TrimEnd(' ');
                UserData.settings["gameArgs"] += " ";
            }
            else
            {
                UserData.settings["gameArgs"] = "";
            }

            if (UpdatesOff.IsChecked == true)
                UserData.settings["noUpdate"] = "true";
            else
                UserData.settings["noUpdate"] = "false";

            if (ShowConsole.IsChecked == true)
                UserData.settings["showConsole"] = "true";
            else
                UserData.settings["showConsole"] = "false";

            if (HiddenMode.IsChecked == true)
                UserData.settings["hiddenMode"] = "true";
            else
                UserData.settings["hiddenMode"] = "false";

            BackMainWindow(null, null);
            await Task.Run(() => WithDirectory.SaveSettings(UserData.settings));
        }

        private void SelectFolder(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            if(!string.IsNullOrWhiteSpace(fbd.SelectedPath))
                dir.Text = fbd.SelectedPath.Replace(@"\", "/");
        }

        private void SelectJavaPath(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OPF = new OpenFileDialog();
            OPF.Filter = "Исполняемый файл java|java.exe|Исполняемый файл java|javaw.exe";
            OPF.ShowDialog();
            JVMPath.Text = OPF.FileName.Replace(@"\", "/");
        }
        */

        /* <-- Кастомное меню --> */
        private void CloseWindow(object sender, RoutedEventArgs e) { this.Close(); }
        private void HideWindow(object sender, RoutedEventArgs e) { this.WindowState = WindowState.Minimized; }
    }
}

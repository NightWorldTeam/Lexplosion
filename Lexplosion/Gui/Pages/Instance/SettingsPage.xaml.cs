using Lexplosion.Global;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic;
using Lexplosion.Logic.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lexplosion.Gui.Pages.Instance
{
    /// <summary>
    /// Логика взаимодействия для SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private string _sysPath;
        private MainWindow _mainWindow;
        private Settings _settings;
        private Settings _settingsCopied;
        private string _instanceId;
        
        public SettingsPage(MainWindow mainWindow, string instanceId)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _instanceId = instanceId;

            _settings = DataFilesManager.GetSettings(instanceId);
            _settingsCopied = _settings.Copy();

            _settings.Merge(UserData.GeneralSettings, true);
            
            SetSettings();
        }

        private void SetSettings()
        {
            WidthTextBox.Text = _settings.WindowWidth.ToString();
            HeightTextBox.Text = _settings.WindowHeight.ToString();
            XmxTextBox.Text = _settings.Xmx.ToString();

            _sysPath = _settings.GamePath.Replace("/", @"\");
            InstanceFolderPath.Text = _sysPath;

            foreach (string resolution in MainWindow.ScreenResolutions)
            {
                ScreenResolutions.Items.Add(resolution);
            }
        }

        private void OpenFolderBrowser(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = _sysPath;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _sysPath = dialog.SelectedPath;
                    InstanceFolderPath.Text = _sysPath;
                    _settingsCopied.GamePath = _sysPath.Replace(@"\", "/");
                    DataFilesManager.SaveSettings(_settingsCopied, _instanceId);
                }
            }
        }

        private void HeightTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _settingsCopied.WindowHeight = uint.Parse(HeightTextBox.Text);
            DataFilesManager.SaveSettings(_settingsCopied, _instanceId);
        }

        private void WidthTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _settingsCopied.WindowWidth = uint.Parse(WidthTextBox.Text);
            DataFilesManager.SaveSettings(_settingsCopied, _instanceId);
        }

        private void WidthTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            WidthTextBox.Text = Regex.Match(WidthTextBox.Text, @"[0-9]+").ToString();
            WidthTextBox.Select(WidthTextBox.Text.Length, 0);
        }

        private void HeightTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HeightTextBox.Text = Regex.Match(HeightTextBox.Text, @"[0-9]+").ToString();
            HeightTextBox.Select(HeightTextBox.Text.Length, 0);
        }

        private void ScreenResolutions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string[] resolution;
            resolution = ScreenResolutions.SelectedItem.ToString().Split('x');

            _settingsCopied.WindowWidth = uint.Parse(resolution[0]);
            _settingsCopied.WindowHeight = uint.Parse(resolution[1]);

            DataFilesManager.SaveSettings(_settingsCopied, _instanceId);
        }

        private void GameFolderPath_LostFocus(object sender, RoutedEventArgs e)
        {
            _settingsCopied.GamePath = GameFolderPath.Text.Replace(@"\", "/");
            InstanceFolderPath.Text = _sysPath;
            DataFilesManager.SaveSettings(_settingsCopied, _instanceId);
        }

        private void InstanceFolderPath_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void XmxTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _settingsCopied.Xmx = uint.Parse(XmxTextBox.Text);
            DataFilesManager.SaveSettings(_settingsCopied, _instanceId);
        }

        private void XmxTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            XmxTextBox.Text = Regex.Match(XmxTextBox.Text, @"[0-9]+").ToString();
            XmxTextBox.Select(XmxTextBox.Text.Length, 0);
        }

        private void ShowConsoleCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _settingsCopied.ShowConsole = true;
            DataFilesManager.SaveSettings(_settingsCopied, _instanceId);
        }

        private void ShowConsoleCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _settingsCopied.ShowConsole = false;
            DataFilesManager.SaveSettings(_settingsCopied, _instanceId);
        }
    }
}

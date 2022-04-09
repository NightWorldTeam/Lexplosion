using Lexplosion.Global;
using Lexplosion.Gui.Windows;
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
        private Dictionary<string, string> _settings;
        private string _instanceId;
        
        public SettingsPage(MainWindow mainWindow, string instanceId)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _instanceId = instanceId;
            _settings = DataFilesManager.GetSettings(instanceId);
            if (_settings.Keys.Count == 0) 
            {
                _settings = UserData.Settings;
                DataFilesManager.SaveSettings(_settings, instanceId);
            }
            SetSettings();
        }

        private void SetSettings()
        {
            WidthTextBox.Text = _settings["windowWidth"];
            HeightTextBox.Text = _settings["windowHeight"];
            XmxTextBox.Text = _settings["xmx"];

            _sysPath = _settings["gamePath"].Replace("/", @"\");
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
                    _settings["gamePath"] = _sysPath.Replace(@"\", "/");
                    DataFilesManager.SaveSettings(_settings, _instanceId);
                }
            }
        }

        private void HeightTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _settings["windowHeight"] = HeightTextBox.Text;
            HeightTextBox.Text = _settings["windowHeight"];
            DataFilesManager.SaveSettings(_settings, _instanceId);
        }

        private void WidthTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _settings["windowWidth"] = WidthTextBox.Text;
            WidthTextBox.Text = _settings["windowWidth"];
            DataFilesManager.SaveSettings(_settings, _instanceId);
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

            _settings["windowWidth"] = resolution[0];
            _settings["windowHeight"] = resolution[1];

            WidthTextBox.Text = _settings["windowWidth"];
            HeightTextBox.Text = _settings["windowHeight"];

            DataFilesManager.SaveSettings(_settings, _instanceId);
        }

        private void GameFolderPath_LostFocus(object sender, RoutedEventArgs e)
        {
            _settings["gamePath"] = GameFolderPath.Text.Replace(@"\", "/");
            _sysPath = _settings["gamePath"].Replace("/", @"\");
            InstanceFolderPath.Text = _sysPath;
            DataFilesManager.SaveSettings(_settings);
        }

        private void InstanceFolderPath_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void XmxTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _settings["xmx"] = XmxTextBox.Text;
            XmxTextBox.Text = _settings["xmx"];
            DataFilesManager.SaveSettings(_settings, _instanceId);
        }

        private void XmxTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            XmxTextBox.Text = Regex.Match(XmxTextBox.Text, @"[0-9]+").ToString();
            XmxTextBox.Select(XmxTextBox.Text.Length, 0);
        }

        private void ShowConsoleCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //_settings["showConsole"] = "true";
        }

        private void ShowConsoleCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //_settings["showConsole"] = "false";
        }
    }
}

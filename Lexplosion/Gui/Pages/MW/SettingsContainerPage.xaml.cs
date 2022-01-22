using Lexplosion.Global;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.FileSystem;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Lexplosion.Gui.Pages.MW
{
    /// <summary>
    /// Interaction logic for SettingsContainerPage.xaml
    /// </summary>
    public partial class SettingsContainerPage : Page
    {
        private string _sysPath;
        private MainWindow _mainWindow;

        private List<string> _screenResolutions = new List<string>()
        {
            "1920x1080", "1768x992", "1680x1050",  "1600x1024", "1600x900", "1440x900", "1280x1024",
            "1280x960", "1366x768", "1360x768", "1280x800", "1280x768", "1152x864", "1280x720", "1176x768",
            "1024x768", "800x600", "720x576", "720x480", "640x480"
        };


        public SettingsContainerPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            SetSettings();
        }

        private void SetSettings()
        {
            WidthTextBox.Text = UserData.settings["windowWidth"];
            HeightTextBox.Text = UserData.settings["windowHeight"];

            _sysPath = UserData.settings["gamePath"].Replace("/", @"\");
            InstanceFolderPath.Text = _sysPath;

            foreach (string resolution in _screenResolutions)
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
                    UserData.settings["gamePath"] = _sysPath.Replace(@"\", "/");
                }
            }
        }

        private void HeightTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UserData.settings["windowHeight"] = HeightTextBox.Text;
            HeightTextBox.Text = UserData.settings["windowHeight"];
            DataFilesManager.SaveSettings(UserData.settings);
        }

        private void WidthTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UserData.settings["windowWidth"] = WidthTextBox.Text;
            WidthTextBox.Text = UserData.settings["windowWidth"];
            DataFilesManager.SaveSettings(UserData.settings);
        }

        private void WidthTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            WidthTextBox.Text = Regex.Match(WidthTextBox.Text, @"[0-9]+").ToString();
            HeightTextBox.Select(WidthTextBox.Text.Length, 0);
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

            UserData.settings["windowWidth"] = resolution[0];
            UserData.settings["windowHeight"] = resolution[1];

            WidthTextBox.Text = UserData.settings["windowWidth"];
            HeightTextBox.Text = UserData.settings["windowHeight"];
        }

        private void GameFolderPath_LostFocus(object sender, RoutedEventArgs e)
        {
            UserData.settings["gamePath"] = GameFolderPath.Text.Replace(@"\", "/");
            _sysPath = UserData.settings["gamePath"].Replace("/", @"\");
            InstanceFolderPath.Text = _sysPath;
            DataFilesManager.SaveSettings(UserData.settings);
        }

        private void InstanceFolderPath_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}

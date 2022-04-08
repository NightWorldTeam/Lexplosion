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

        public SettingsContainerPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            SetSettings();
        }

        private void SetSettings()
        {
            WidthTextBox.Text = UserData.Settings["windowWidth"];
            HeightTextBox.Text = UserData.Settings["windowHeight"];
            XmxTextBox.Text = UserData.Settings["xmx"];

            _sysPath = UserData.Settings["gamePath"].Replace("/", @"\");
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
                    UserData.Settings["gamePath"] = _sysPath.Replace(@"\", "/");
                }
            }
        }

        private void HeightTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UserData.Settings["windowHeight"] = HeightTextBox.Text;
            HeightTextBox.Text = UserData.Settings["windowHeight"];
            DataFilesManager.SaveSettings(UserData.Settings);
        }

        private void WidthTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UserData.Settings["windowWidth"] = WidthTextBox.Text;
            WidthTextBox.Text = UserData.Settings["windowWidth"];
            DataFilesManager.SaveSettings(UserData.Settings);
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

            UserData.Settings["windowWidth"] = resolution[0];
            UserData.Settings["windowHeight"] = resolution[1];

            WidthTextBox.Text = UserData.Settings["windowWidth"];
            HeightTextBox.Text = UserData.Settings["windowHeight"];
        }

        private void GameFolderPath_LostFocus(object sender, RoutedEventArgs e)
        {
            UserData.Settings["gamePath"] = GameFolderPath.Text.Replace(@"\", "/");
            _sysPath = UserData.Settings["gamePath"].Replace("/", @"\");
            InstanceFolderPath.Text = _sysPath;
            DataFilesManager.SaveSettings(UserData.Settings);
        }

        private void InstanceFolderPath_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void XmxTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UserData.Settings["xmx"] = XmxTextBox.Text;
            XmxTextBox.Text = UserData.Settings["xmx"];
            DataFilesManager.SaveSettings(UserData.Settings);
        }

        private void XmxTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            XmxTextBox.Text = Regex.Match(XmxTextBox.Text, @"[0-9]+").ToString();
            XmxTextBox.Select(XmxTextBox.Text.Length, 0);
        }

        private void ShowConsoleCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UserData.Settings["showConsole"] = "true";
            DataFilesManager.SaveSettings(UserData.Settings);
        }

        private void ShowConsoleCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UserData.Settings["showConsole"] = "false";
            DataFilesManager.SaveSettings(UserData.Settings);
        }
    }
}

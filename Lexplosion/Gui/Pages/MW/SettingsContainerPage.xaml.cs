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
            WidthTextBox.Text = UserData.GeneralSettings.WindowWidth.ToString();
            HeightTextBox.Text = UserData.GeneralSettings.WindowHeight.ToString();
            XmxTextBox.Text = UserData.GeneralSettings.Xmx.ToString();
            ShowConsoleCheckBox.IsChecked = UserData.GeneralSettings.ShowConsole;

            _sysPath = UserData.GeneralSettings.GamePath.Replace("/", @"\");
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
                    UserData.GeneralSettings.GamePath = _sysPath.Replace(@"\", "/");
                }
            }
        }

        private void HeightTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UserData.GeneralSettings.WindowHeight = uint.Parse(HeightTextBox.Text);
            HeightTextBox.Text = UserData.GeneralSettings.WindowHeight.ToString();
            DataFilesManager.SaveSettings(UserData.GeneralSettings);
        }

        private void WidthTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UserData.GeneralSettings.WindowWidth = uint.Parse(WidthTextBox.Text);
            WidthTextBox.Text = UserData.GeneralSettings.WindowWidth.ToString();
            DataFilesManager.SaveSettings(UserData.GeneralSettings);
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

            UserData.GeneralSettings.WindowWidth = uint.Parse(resolution[0]);
            UserData.GeneralSettings.WindowHeight = uint.Parse(resolution[1]);

            WidthTextBox.Text = UserData.GeneralSettings.WindowWidth.ToString();
            HeightTextBox.Text = UserData.GeneralSettings.WindowHeight.ToString();
        }

        private void GameFolderPath_LostFocus(object sender, RoutedEventArgs e)
        {
            UserData.GeneralSettings.GamePath = GameFolderPath.Text.Replace(@"\", "/");
            _sysPath = UserData.GeneralSettings.GamePath.Replace("/", @"\");
            InstanceFolderPath.Text = _sysPath;
            DataFilesManager.SaveSettings(UserData.GeneralSettings);
        }

        private void InstanceFolderPath_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void XmxTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UserData.GeneralSettings.Xmx = uint.Parse(XmxTextBox.Text);
            XmxTextBox.Text = UserData.GeneralSettings.Xmx.ToString();
            DataFilesManager.SaveSettings(UserData.GeneralSettings);
        }

        private void XmxTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            XmxTextBox.Text = Regex.Match(XmxTextBox.Text, @"[0-9]+").ToString();
            XmxTextBox.Select(XmxTextBox.Text.Length, 0);
        }

        private void ShowConsoleCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UserData.GeneralSettings.ShowConsole = true;
            DataFilesManager.SaveSettings(UserData.GeneralSettings);
        }

        private void ShowConsoleCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UserData.GeneralSettings.ShowConsole = false;
            DataFilesManager.SaveSettings(UserData.GeneralSettings);
        }
    }
}

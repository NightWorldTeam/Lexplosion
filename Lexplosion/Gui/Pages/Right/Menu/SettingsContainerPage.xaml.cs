using Lexplosion.Gui.Windows;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Lexplosion.Gui.Pages.Right.Menu
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsContainerPage : Page
    {
        private List<string> _screenResolutions = new List<string>()
        {
            "1920x1080", "1768x992", "1680x1050",  "1600x1024", "1600x900", "1440x900", "1280x1024", 
            "1280x960", "1366x768", "1360x768", "1280x800", "1280x768", "1152x864", "1280x720", "1176x768",
            "1024x768", "800x600", "720x576", "720x480", "640x480"
        };

        public SettingsContainerPage(MainWindow mainWindow)
        {
            InitializeComponent();
            foreach (string resolution in _screenResolutions) 
            {
                ScreenResolutions.Items.Add(resolution);
            }
        }
    }
}

using Lexplosion.Gui.Windows;
using System.Windows.Controls;

namespace Lexplosion.Gui.InstanceCreator
{
    /// <summary>
    /// Interaction logic for InstanceSettingsPage.xaml
    /// </summary>
    public partial class InstanceSettingsPage : Page
    {
        private MainWindow _mainWindow;
        public InstanceSettingsPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }
    }
}

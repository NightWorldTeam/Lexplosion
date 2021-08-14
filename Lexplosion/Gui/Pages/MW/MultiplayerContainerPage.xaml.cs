using Lexplosion.Gui.Windows;
using System.Windows.Controls;

namespace Lexplosion.Gui.Pages.MW
{
    /// <summary>
    /// Interaction logic for ServersContainerPage.xaml
    /// </summary>
    public partial class MultiplayerContainerPage : Page
    {
        private MainWindow _mainWindow;

        public MultiplayerContainerPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }
    }
}

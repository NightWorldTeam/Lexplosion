using Lexplosion.Gui.Windows;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.Gui.Pages.Instance
{
    /// <summary>
    /// Interaction logic for InstancePage.xaml
    /// </summary>
    public partial class InstancePage : Page
    {
        //private ToggleButton selectedToggleButton;
        public static InstancePage obj = null;
        private MainWindow _mainWindow;

        //private string title;
        //private string description;

        public InstancePage(MainWindow mainWindow)
        {
            InitializeComponent();
            obj = this;
            _mainWindow = mainWindow;
        }

        private void ClickedOverview(object sender, RoutedEventArgs e)
        {
            //BottomSideFrame.Navigate(new OverviewPage(this.title, this.description));
        }

        private void ClickedModsList(object sender, RoutedEventArgs e)
        {
            //BottomSideFrame.Navigate(new ModsListPage(this));
        }

        private void ClickedVersion(object sender, RoutedEventArgs e)
        {
            //BottomSideFrame.Navigate(new VersionPage(this));
        }

        private void СlientManager(object sender, RoutedEventArgs e)
        {
            //if (LeftSideMenuPage.instance.selectedInstance != "")
            //    ManageLogic.СlientManager(LeftSideMenuPage.instance.selectedInstance);
        }
    }
}
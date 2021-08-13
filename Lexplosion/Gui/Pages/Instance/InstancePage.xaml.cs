using Lexplosion.Gui.UserControls;
using Lexplosion.Gui.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lexplosion.Gui.Pages.Instance
{
    /// <summary>
    /// Interaction logic for InstancePage.xaml
    /// </summary>
    public partial class InstancePage : Page
    {
        //private ToggleButton selectedToggleButton;
        public static InstancePage instance = null;
        private MainWindow _mainWindow;

        //private string title;
        //private string description;

        public InstancePage(MainWindow mainWindow)
        {
            InitializeComponent();
            InitializeLeftPanel();
            _mainWindow = mainWindow;
        }

        private void InitializeLeftPanel() 
        {
            LeftPanel leftPanel = new LeftPanel(this, LeftPanel.PageType.OpenedInstance, _mainWindow);
            Grid.SetColumn(leftPanel, 0);
            MainGrid.Children.Add(leftPanel);
        }

        private void ReselectionToggleButton(object sender)
        {
            //ToggleButton toggleButton = (ToggleButton)sender;
            //if (toggleButton.Name != selectedToggleButton.Name)
            //{
            //    toggleButton.IsChecked = true;
            //    selectedToggleButton.IsChecked = false;
            //    selectedToggleButton = toggleButton;
            //}
            //else toggleButton.IsChecked = true;
        }

        private void ClickedOverview(object sender, RoutedEventArgs e)
        {
            //ReselectionToggleButton(sender);
            //BottomSideFrame.Navigate(new OverviewPage(this.title, this.description));
        }

        private void ClickedModsList(object sender, RoutedEventArgs e)
        {
            //ReselectionToggleButton(sender);
            //BottomSideFrame.Navigate(new ModsListPage(this));
        }

        private void ClickedVersion(object sender, RoutedEventArgs e)
        {
            //ReselectionToggleButton(sender);
            //BottomSideFrame.Navigate(new VersionPage(this));
        }

        private void СlientManager(object sender, RoutedEventArgs e)
        {
            //if (LeftSideMenuPage.instance.selectedInstance != "")
            //    ManageLogic.СlientManager(LeftSideMenuPage.instance.selectedInstance);

        }
    }
}
using Lexplosion.Gui.Windows;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Lexplosion.Gui.Pages.Instance
{
    /// <summary>
    /// Interaction logic for InstancePage.xaml
    /// </summary>
    public partial class InstancePage : Page
    {
        public static InstancePage obj = null;
        private MainWindow _mainWindow;

        private List<ToggleButton> toggleButtons = new List<ToggleButton>();


        //private string title;
        //private string description;

        public InstancePage(MainWindow mainWindow)
        {
            InitializeComponent();
            obj = this;
            _mainWindow = mainWindow;
            toggleButtons.Add(OverviewToggleButton);
            toggleButtons.Add(ModsToggleButton);
            toggleButtons.Add(VersionToggleButton);
        }

        private void ClickedOverview(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(typeof(OverviewPage).ToString());
            //_mainWindow.PagesController<OverviewPage>("OverviewPage", BottomSideFrame);
            ReselectionButton(OverviewToggleButton);
        }

        private void ClickedModsList(object sender, RoutedEventArgs e)
        {
            ReselectionButton(ModsToggleButton);
            //BottomSideFrame.Navigate(new ModsListPage(this));
        }

        private void ClickedVersion(object sender, RoutedEventArgs e)
        {
            ReselectionButton(VersionToggleButton);
            //BottomSideFrame.Navigate(new VersionPage(this));
        }

        private void СlientManager(object sender, RoutedEventArgs e)
        {
            //if (LeftSideMenuPage.instance.selectedInstance != "")
            //    ManageLogic.СlientManager(LeftSideMenuPage.instance.selectedInstance);
        }

        private void ReselectionButton(ToggleButton selectedButton)
        {
            foreach (ToggleButton toggleButton in toggleButtons)
            {
                if (toggleButton != selectedButton)
                {
                    toggleButton.IsEnabled = true;
                    toggleButton.IsChecked = false;
                }
                else
                {
                    selectedButton.IsChecked = true;
                    selectedButton.IsEnabled = false;
                }
            }
        }
    }
}
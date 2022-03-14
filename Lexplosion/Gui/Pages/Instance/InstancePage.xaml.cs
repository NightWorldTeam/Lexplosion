using Lexplosion.Gui.Windows;
using Lexplosion.Logic.Objects;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Lexplosion.Gui.Pages.Instance
{
    /// <summary>
    /// Interaction logic for InstancePage.xaml
    /// </summary>
    public partial class InstancePage : Page
    {
        public static InstancePage Obj = null;
        private MainWindow _mainWindow;
        private List<ToggleButton> _toggleButtons = new List<ToggleButton>();
        public InstanceProperties _instanceProperties;


        public InstancePage(MainWindow mainWindow, InstanceProperties instanceProperties)
        {
            InitializeComponent();
            Obj = this;
            _mainWindow = mainWindow;
            _instanceProperties = instanceProperties;
            ActivateButtons();

            InstanceName.Text = instanceProperties.Name;
            InstanceLogo.Fill = new ImageBrush(_instanceProperties.Logo);

            mainWindow.PagesController("OverviewPage" + instanceProperties.Id, this.BottomSideFrame, delegate ()
            {
                return new OverviewPage(instanceProperties);
            });
            OverviewToggleButton.IsChecked = true;
            OverviewToggleButton.IsEnabled = true;
        }

        public static InstanceProperties GetInstanceProperties() => Obj._instanceProperties;   

        private void ActivateButtons() 
        {
            _toggleButtons.Add(OverviewToggleButton);
            _toggleButtons.Add(ModsToggleButton);
            _toggleButtons.Add(VersionToggleButton);
        }

        private void ClickedOverview(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(typeof(OverviewPage).ToString());
            _mainWindow.PagesController("OverviewPage" + _instanceProperties.Id, this.BottomSideFrame, delegate ()
            {
                return new OverviewPage(_instanceProperties);
            });
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
            foreach (ToggleButton toggleButton in _toggleButtons)
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
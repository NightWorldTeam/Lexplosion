using Lexplosion.Gui.Pages.Left;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.Management;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Lexplosion.Gui.Pages.Right.Instance
{
    /// <summary>
    /// Interaction logic for InstancePage.xaml
    /// </summary>
    public partial class InstancePage : Page
    {
        private ToggleButton selectedToggleButton;
        public static InstancePage instance = null;

        private string title;
        private string description;

        public InstancePage(string title, string description)
        {
            InitializeComponent();
            instance = this;
            InstanceTitle.Text = title;
            BottomSideFrame.Navigate(new OverviewPage(title, description));
            selectedToggleButton = OverviewToggleButton;
            selectedToggleButton.IsChecked = true;
        }

        private void ReselectionToggleButton(object sender)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            if (toggleButton.Name != selectedToggleButton.Name)
            {
                toggleButton.IsChecked = true;
                selectedToggleButton.IsChecked = false;
                selectedToggleButton = toggleButton;
            }
            else toggleButton.IsChecked = true;
        }

        private void ClickedOverview(object sender, RoutedEventArgs e)
        {
            ReselectionToggleButton(sender);
            BottomSideFrame.Navigate(new OverviewPage(this.title, this.description));
        }

        private void ClickedModsList(object sender, RoutedEventArgs e)
        {
            ReselectionToggleButton(sender);
            BottomSideFrame.Navigate(new ModsListPage(this));
        }

        private void ClickedVersion(object sender, RoutedEventArgs e)
        {
            ReselectionToggleButton(sender);
            BottomSideFrame.Navigate(new VersionPage(this));
        }

        private void СlientManager(object sender, RoutedEventArgs e)
        {
            if(LeftSideMenuPage.instance.selectedInstance != "")
                ManageLogic.СlientManager(LeftSideMenuPage.instance.selectedInstance);

        }
    }
}

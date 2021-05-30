using Lexplosion.Logic.Management;
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

        public InstancePage()
        {
            InitializeComponent();

            selectedToggleButton = OverviewToggleButton;
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
        }

        private void ClickedModsList(object sender, RoutedEventArgs e)
        {
            ReselectionToggleButton(sender);
        }

        private void ClickedVersion(object sender, RoutedEventArgs e)
        {
            ReselectionToggleButton(sender);
        }



        private void СlientManager(object sender, RoutedEventArgs e)
        {
            if(LeftSideMenuPage.instance.selectedInstance != "")
                ManageLogic.СlientManager(LeftSideMenuPage.instance.selectedInstance, Logic.InstanceType.Nightworld);

        }
    }
}

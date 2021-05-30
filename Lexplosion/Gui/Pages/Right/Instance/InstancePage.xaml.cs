using Lexplosion.Logic.Management;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.Gui.Pages.Right.Instance
{
    /// <summary>
    /// Interaction logic for InstancePage.xaml
    /// </summary>
    public partial class InstancePage : Page
    {
        public InstancePage()
        {
            InitializeComponent();
        }

        private void OverviewClick(object sender, RoutedEventArgs e)
        {

        }

        private void VersionClick(object sender, RoutedEventArgs e)
        {

        }

        private void ModsListClick(object sender, RoutedEventArgs e)
        {

        }

        private void СlientManager(object sender, RoutedEventArgs e)
        {
            if(LeftSideMenuPage.instance.selectedInstance != "")
                ManageLogic.СlientManager(LeftSideMenuPage.instance.selectedInstance);

        }
    }
}

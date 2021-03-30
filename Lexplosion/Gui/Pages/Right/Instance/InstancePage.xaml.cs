using Lexplosion.Logic.Management;
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
            if(LeftSideMenuPage.selectedInstance != "")
                ManageLogic.СlientManager(LeftSideMenuPage.selectedInstance);

        }
    }
}

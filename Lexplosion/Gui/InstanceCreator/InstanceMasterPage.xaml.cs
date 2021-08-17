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

namespace Lexplosion.Gui.InstanceCreator
{
    /// <summary>
    /// Interaction logic for InstanceMasterPage.xaml
    /// </summary>
    public partial class InstanceMasterPage : Page
    {
        private MainWindow _mainWindow;
        public InstanceMasterPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void Main_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.PagesController<InstanceCreateMainPage>("InstanceCreateMainPage", this.BottomSideFrame);
        }

        private void Mods_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.PagesController<InstanceCreateModsPage>("InstanceCreateModsPage", this.BottomSideFrame);
        }

        private void Resourcepacks_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.PagesController<InstanceCreateResourcepacksPage>("InstanceCreateResourcepacksPage", this.BottomSideFrame);
        }

        private void Shaderspacks_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.PagesController<InstanceCreateShaderspacksPage>("InstanceCreateResourcepacksPage", this.BottomSideFrame);
        }
    }
}

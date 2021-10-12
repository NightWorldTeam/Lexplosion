using Lexplosion.Gui.UserControls;
using Lexplosion.Gui.Windows;
using System.Windows;
using System.Windows.Controls;

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
            LeftPanel.AddModpackClicked += SetDefaultPage;
        }

        private void Main_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.PagesController("InstanceCreateMainPage", this.BottomSideFrame, delegate ()
            {
                return new InstanceCreateMainPage();
            });
        }

        private void Mods_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.PagesController("InstanceCreateModsPage", this.BottomSideFrame, delegate ()
            {
                return new InstanceCreateModsPage();
            });
        }

        private void Resourcepacks_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.PagesController("InstanceCreateResourcepacksPage", this.BottomSideFrame, delegate ()
            {
                return new InstanceCreateResourcepacksPage();
            });
        }

        private void Shaderspacks_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.PagesController("InstanceCreateShaderspacksPage", this.BottomSideFrame, delegate ()
            {
                return new InstanceCreateShaderspacksPage();
            });
        }

        private void SetDefaultPage() 
        {
            _mainWindow.PagesController("InstanceCreateMainPage", this.BottomSideFrame, delegate ()
            {
                return new InstanceCreateMainPage();
            });
        }
    }
}

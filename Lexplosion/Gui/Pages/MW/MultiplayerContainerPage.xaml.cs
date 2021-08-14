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

namespace Lexplosion.Gui.Pages.MW
{
    /// <summary>
    /// Interaction logic for ServersContainerPage.xaml
    /// </summary>
    public partial class MultiplayerContainerPage : Page
    {
        public static MultiplayerContainerPage obj = null;
        private MainWindow _mainWindow;

        public MultiplayerContainerPage(MainWindow mainWindow)
        {
            InitializeComponent();
            obj = this;
            _mainWindow = mainWindow;
        }
    }
}

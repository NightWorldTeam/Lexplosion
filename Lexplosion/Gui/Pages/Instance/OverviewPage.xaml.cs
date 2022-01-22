using Lexplosion.Logic.Objects;
using System.Windows.Controls;

namespace Lexplosion.Gui.Pages.Instance
{
    /// <summary>
    /// Interaction logic for OverviewPage.xaml
    /// </summary>
    public partial class OverviewPage : Page
    {
        private InstanceProperties _instaceProperties;
        public OverviewPage(InstanceProperties instanceProperties)
        {
            InitializeComponent();
            _instaceProperties = instanceProperties;
        }
    }
}
using Lexplosion.Logic.Network;
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
            var x = ToServer.HttpGet("https://addons-ecs.forgesvc.net/api/v2/addon/429793/description");
            webb.NavigateToString(x);
        }
    }
}
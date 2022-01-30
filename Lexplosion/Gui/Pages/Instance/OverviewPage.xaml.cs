using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using System;
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

        private void CurseforgeUrl_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Console.WriteLine(1);
            System.Diagnostics.Process.Start("https://www.curseforge.com/minecraft/modpacks/rlcraft");
        }
    }
}
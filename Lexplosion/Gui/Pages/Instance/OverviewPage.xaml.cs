using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using System;
using System.Windows;
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
            Gallery.LoadImages(instanceProperties.InstanceAssets.images);
        }

        private void CurseforgeUrl_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.curseforge.com/minecraft/modpacks/rlcraft");
        }
    }
}
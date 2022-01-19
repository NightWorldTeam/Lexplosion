using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Lexplosion.Gui.Pages.Instance
{
    /// <summary>
    /// Interaction logic for OverviewPage.xaml
    /// </summary>
    public partial class OverviewPage : Page
    {
        private InstanceProperties instaceProperties;
        public OverviewPage(InstanceProperties instanceProperties)
        {
            InitializeComponent();
            this.instaceProperties = instaceProperties;
        }


    }
}
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Lexplosion.Logic.Objects;
using Lexplosion.UI.WPF.Core;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.MainContent.ServerProfile
{
    /// <summary>
    /// Interaction logic for ServerProfileLeftPanelView.xaml
    /// </summary>
    public partial class ServerProfileLeftPanelView : UserControl
    {
        public ServerProfileLeftPanelView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Social Media Clicked
        /// </summary>
        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var border = (Border)sender;
            var url = ((Link)border.DataContext).Url;
            Process.Start(url);
        }
    }
}

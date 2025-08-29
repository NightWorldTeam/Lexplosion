using System.Diagnostics;
using System.Windows.Controls;
using Lexplosion.Logic.Objects;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.MainContent.ServerProfile
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

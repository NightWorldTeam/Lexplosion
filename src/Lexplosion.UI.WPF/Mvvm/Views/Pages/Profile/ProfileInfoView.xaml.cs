using Lexplosion.UI.WPF.Mvvm.Models.Profile;
using System.Diagnostics;
using System.Windows.Controls;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.Profile
{
    /// <summary>
    /// Interaction logic for ProfileInfo.xaml
    /// </summary>
    public partial class ProfileInfoView : UserControl
    {
        public ProfileInfoView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Social Media Clicked
        /// </summary>
        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var border = (Border)sender;
            var url = ((ProfileSocialMedia)border.DataContext).Url;
            Process.Start(url);
        }
    }
}

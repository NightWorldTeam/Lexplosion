using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu;
using Lexplosion.WPF.NewInterface.Tools;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.MainContent.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для LeftPanelView.xaml
    /// </summary>
    public partial class LeftPanelView : UserControl
    {
        public LeftPanelView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null)
            {
                UpdateBannerProperties((LeftPanelViewModel)DataContext);
            }
        }

        private void UpdateBannerProperties(LeftPanelViewModel leftPanelViewModel) 
        {
            var profileBanner = leftPanelViewModel.ProfileBanner;

            if (profileBanner == null || profileBanner.Colors == null) 
            {
                UserNickname.SetResourceReference(ForegroundProperty, "PrimaryForegroundSolidColorBrush");
                GladToSeeYouText.SetResourceReference(ForegroundProperty, "SecondaryForegroundSolidColorBrush");
                return;
            }

            if (profileBanner.Colors.PrimaryForeColor != null && profileBanner.Colors.PrimaryForeColor > 0x01000000) 
            {
                UserNickname.Foreground = (SolidColorBrush)new SolidColorBrush(ColorTools.GetColor(profileBanner.Colors.PrimaryForeColor.Value));
            }

            if (profileBanner.Colors.SecondaryForeColor != null && profileBanner.Colors.PrimaryForeColor > 0x01000000)
            {
                GladToSeeYouText.Foreground = (SolidColorBrush)new SolidColorBrush(ColorTools.GetColor(profileBanner.Colors.SecondaryForeColor.Value));
            }
        }
    }
}

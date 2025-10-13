using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.Profile
{
    /// <summary>
    /// Interaction logic for ProfileSettingsView.xaml
    /// </summary>
    public partial class ProfileSettingsView : UserControl
    {
        public ProfileSettingsView()
        {
            InitializeComponent();
        }


        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RecalculateGrid(e.NewSize.Width, e.NewSize.Height);
        }

        private void RecalculateGrid(double width, double height)
        {
            if (width > 672)
            {
                Resources["StatsFontSize"] = 14.0;
                Resources["StatsHighlightPadding"] = new Thickness(16, 0, 16, 0);

                var coverWidth = (double)(int)(width * 0.28);
                Resources["CoverWidth"] = coverWidth;
                Resources["CoverHeight"] = (double)(int)(coverWidth * 0.56);
            }
            else
            {
                Resources["StatsFontSize"] = 13.0;
                Resources["StatsHighlightPadding"] = new Thickness(8, 0, 8, 0);

                Resources["CoverWidth"] = 170.0;
                Resources["CoverHeight"] = 95.0;
            }
        }
    }
}

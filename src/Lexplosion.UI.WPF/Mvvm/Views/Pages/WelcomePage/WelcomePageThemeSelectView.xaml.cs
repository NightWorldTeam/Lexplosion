using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages
{
    /// <summary>
    /// Interaction logic for WelcomePageThemeSelectView.xaml
    /// </summary>
    public partial class WelcomePageThemeSelectView : UserControl
    {
        public WelcomePageThemeSelectView()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            var da = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5)
            };

            grid.BeginAnimation(OpacityProperty, da);
        }
    }
}

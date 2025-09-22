using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.Authorization
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationMenuView.xaml
    /// </summary>
    public partial class AuthorizationMenuView : UserControl
    {
        public AuthorizationMenuView()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, System.Windows.RoutedEventArgs e)
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

using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.MainContent.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для MainMenuLayoutViewModel.xaml
    /// </summary>
    public partial class MainMenuLayoutView : UserControl
    {
        public MainMenuLayoutView()
        {
            InitializeComponent();
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var actualHeight = ((Grid)sender).ActualHeight;
            var actualWidth = ((Grid)sender).ActualWidth;

            var grid = (Grid)sender;


            foreach (var i in grid.ColumnDefinitions)
            {
                Runtime.DebugWrite(i.ActualWidth);
            }
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

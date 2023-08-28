using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Views.Pages.MainContent.MainMenu
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
    }
}

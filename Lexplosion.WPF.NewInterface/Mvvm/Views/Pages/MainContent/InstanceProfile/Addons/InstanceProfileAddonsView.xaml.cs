using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.MainContent.InstanceProfile
{
    /// <summary>
    /// Логика взаимодействия для InstanceProfileAddonsView.xaml
    /// </summary>
    public partial class InstanceProfileAddonsView : UserControl
    {
        public InstanceProfileAddonsView()
        {
            InitializeComponent();
        }

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var grid = (Grid)sender;
            Runtime.DebugWrite(grid.ActualWidth.ToString());
            for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
            {
                Runtime.DebugWrite(i.ToString() + " " + grid.ColumnDefinitions[i].ActualWidth.ToString());
            }
        }

        private void ListBox_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var grid = (FrameworkElement)sender;
            Runtime.DebugWrite(" lsb: " + grid.ActualWidth.ToString());
            Runtime.DebugWrite(" usercontrol: " + this.ActualWidth.ToString());
        }
    }
}

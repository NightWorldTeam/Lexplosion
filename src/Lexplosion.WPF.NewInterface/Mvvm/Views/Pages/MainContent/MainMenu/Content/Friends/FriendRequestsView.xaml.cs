using System.Diagnostics;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.MainContent.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для FriendsRequestsView.xaml
    /// </summary>
    public partial class FriendsRequestsView : UserControl
    {
        public FriendsRequestsView()
        {
            InitializeComponent();
        }

        private void ItemsControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            var itemsControl = sender as ItemsControl;

            Runtime.DebugWrite($"ItemsControl weight x height {itemsControl.ActualWidth}x{itemsControl.ActualHeight}");
            Runtime.DebugWrite($"ItemsControl items count {itemsControl.Items.Count}");
            Runtime.DebugWrite($"ItemsControl items count {itemsControl.Items.Count}");
        }
    }
}

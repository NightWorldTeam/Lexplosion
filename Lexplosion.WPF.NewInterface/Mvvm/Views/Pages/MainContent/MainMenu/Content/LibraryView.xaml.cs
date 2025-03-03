using Lexplosion.WPF.NewInterface.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using ListBox = System.Windows.Controls.ListBox;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.MainContent.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для LibraryView.xaml
    /// </summary>
    public partial class LibraryView : System.Windows.Controls.UserControl
    {
        double filterHeight = 0;
        bool _isFilterHidden = false;

        public LibraryView()
        {
            InitializeComponent();
            filterHeight = FiltersControlPanel.ActualHeight;
        }

        public static ChildItem FindVisualChild<ChildItem>(DependencyObject obj) where ChildItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is ChildItem)
                    return (ChildItem)child;
                else
                {
                    ChildItem childOfChild = FindVisualChild<ChildItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
        private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (BackTopButton.TargetScroll == null) 
            {
                BackTopButton.TargetScroll = e.OriginalSource as ScrollViewer;
            }
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var grid = sender as Grid;

            var actualHeight = grid.ActualHeight;
            var actualWidth = grid.ActualWidth;



            foreach (var i in grid.ColumnDefinitions)
            {
                Runtime.DebugWrite(i.ActualWidth);
            }
        }

        private void InstanceForm_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var grid = sender as InstanceForm;

            var actualHeight = grid.ActualHeight;
            var actualWidth = grid.ActualWidth;

            Runtime.DebugWrite($"{actualWidth} x {actualHeight}");
        }
    }
}

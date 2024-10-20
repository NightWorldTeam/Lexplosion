using Lexplosion.WPF.NewInterface.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.MainContent.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для LibraryView.xaml
    /// </summary>
    public partial class LibraryView : UserControl
    {
        double filterHeight = 0;
        bool _isFilterHidden = false;

        public LibraryView()
        {
            InitializeComponent();
            filterHeight = FiltersControlPanel.ActualHeight;
        }

        private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

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

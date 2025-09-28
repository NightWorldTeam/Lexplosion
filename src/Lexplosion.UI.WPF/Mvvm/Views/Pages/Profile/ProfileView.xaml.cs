using Lexplosion.UI.WPF.Mvvm.Models.Profile;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.Profile
{
    /// <summary>
    /// Interaction logic for ProfileView.xaml
    /// </summary>
    public partial class ProfileView : UserControl
    {
        public ProfileView()
        {
            InitializeComponent();
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Console.WriteLine($"{e.NewSize.Width} {e.NewSize.Height}");
            RecalculateGrid(e.NewSize.Width, e.NewSize.Height);
        }

        private void RecalculateGrid(double width, double height)
        {
            // 32 + 240 + 400 + 225
            if (width > 750)
            {
                RecalculateGridHorizontal();
            }
            else
            {
                RecalculateGridVertical();
            }

            if (width > 672)
            {
                Resources["StatsFontSize"] = 14.0;
                Resources["StatsHighlightPadding"] = new Thickness(16, 0, 16, 0);
            }
            else
            {
                Resources["StatsFontSize"] = 13.0;
                Resources["StatsHighlightPadding"] = new Thickness(8, 0, 8, 0);
            }
        }

        private void RecalculateGridHorizontal()
        {
            MainContentGrid.ColumnDefinitions.Clear();
            MainContentGrid.RowDefinitions.Clear();
            MainContentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.5, GridUnitType.Star), MinWidth = 400 });
            MainContentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto, MaxWidth = 300, MinWidth = 300 });

            var itemsCount = MainContentGrid.Children.Count;

            for (var i = 0; i < itemsCount; i++)
            {
                var child = MainContentGrid.Children[i];

                if (i < itemsCount - 1)
                {
                    (child as FrameworkElement).Margin = new Thickness(0, 0, 8, 0);
                }

                Grid.SetColumn(child, i);
            }
        }

        private void RecalculateGridVertical()
        {
            MainContentGrid.ColumnDefinitions.Clear();
            MainContentGrid.RowDefinitions.Clear();
            MainContentGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            MainContentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            var itemsCount = MainContentGrid.Children.Count;

            for (var i = 0; i < itemsCount; i++)
            {
                var child = MainContentGrid.Children[itemsCount - i - 1];

                if (i < itemsCount - 1)
                {
                    (child as FrameworkElement).Margin = new Thickness(0, 0, 0, 8);
                }

                Grid.SetRow(child, i);
            }
        }
    }
}

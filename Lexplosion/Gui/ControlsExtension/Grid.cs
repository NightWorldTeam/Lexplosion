using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.Gui.Extension
{
    public class GridHelpers
    {
        public static readonly DependencyProperty RowCountProperty =
            DependencyProperty.RegisterAttached(
                "RowCount", typeof(int), typeof(GridHelpers), new PropertyMetadata(-1, RowCountChanged)
                );
        //public static readonly DependencyProperty RowHeightProperty =
        //    DependencyProperty.RegisterAttached(
        //        "RowHeight", typeof(int), typeof(GridHelpers), new PropertyMetadata(-1, RowCountChanged)
        //    );

        public static int GetRowCount(DependencyObject obj)
        {
            return (int)obj.GetValue(RowCountProperty);
        }

        public static void SetRowCount(DependencyObject obj, int value)
        {
            obj.SetValue(RowCountProperty, value);
        }

        //public static int GetRowHeight(DependencyObject obj) 
        //{
        //    return (int)obj.GetValue(RowHeightProperty);
        //}

        //public static void SetRowHeight(DependencyObject obj, int value) 
        //{
        //    obj.SetValue(RowHeightProperty, value);
        //}

        public static void RowCountChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) 
        {
            if (!(obj is Grid) || (int)e.NewValue < 0)
                return;

            Grid grid = (Grid)obj;
            grid.RowDefinitions.Clear();
            for (int i = 0; i < (int)e.NewValue; i++) 
            {
                //grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(GetRowHeight(grid), GridUnitType.Pixel) });
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(150, GridUnitType.Pixel) });
            }
        }
    }
}

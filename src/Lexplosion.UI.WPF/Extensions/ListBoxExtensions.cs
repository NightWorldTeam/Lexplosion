using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.UI.WPF.Extensions
{
    public static class ListBoxExtensions
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached("SelectedItems", typeof(IList), typeof(ListBoxExtensions),
                new FrameworkPropertyMetadata(null, OnSelectedItemsChanged));

        public static void SetSelectedItems(DependencyObject dp, IList value)
        {
            dp.SetValue(SelectedItemsProperty, value);
        }

        public static IList GetSelectedItems(DependencyObject dp)
        {
            return (IList)dp.GetValue(SelectedItemsProperty);
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListBox listBox)
            {
                listBox.SelectionChanged += ListBox_SelectionChanged;
            }
        }

        private static void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (sender as ListBox);

            foreach (var i in e.RemovedItems) 
            {
                listBox.SelectedItems.Remove(i);
            }

            foreach (var i in e.AddedItems) 
            {
                listBox.SelectedItems.Add(i);
            } 
        }
    }
}

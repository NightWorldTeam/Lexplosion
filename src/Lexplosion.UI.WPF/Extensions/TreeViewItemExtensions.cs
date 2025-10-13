using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Extensions
{
    public static class TreeViewItemExtensions
    {
        public static DependencyProperty ExpandCommandProperty =
            DependencyProperty.RegisterAttached(
                "ExpandCommand",
                typeof(ICommand),
                typeof(TreeViewItem),
                new UIPropertyMetadata(OnCommandChanged));

        public static DependencyProperty ExpandCommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "ExpandCommandParameter",
                typeof(object),
                typeof(TreeViewItem),
                new UIPropertyMetadata(null));

        public static void SetExpandCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(ExpandCommandProperty, value);
        }

        public static ICommand GetExpandCommand(DependencyObject d, ICommand value)
        {
            return (ICommand)d.GetValue(ExpandCommandProperty);
        }

        public static void SetCommandParameter(DependencyObject target, object value)
        {
            target.SetValue(ExpandCommandParameterProperty, value);
        }

        public static object GetCommandParameter(DependencyObject target)
        {
            return target.GetValue(ExpandCommandParameterProperty);
        }

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            System.Windows.Controls.TreeViewItem control = d as System.Windows.Controls.TreeViewItem;

            if (control == null)
                return;

            if (e.NewValue != null && e.OldValue == null)
            {
                control.Expanded += OnExpanded;
            }
            else if (e.NewValue == null && e.OldValue != null)
            {
                control.Expanded -= OnExpanded;
            }
        }

        private static void OnExpanded(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.TreeViewItem control = sender as System.Windows.Controls.TreeViewItem;
            ICommand command = (ICommand)control.GetValue(ExpandCommandProperty);
            object commandParameter = control.GetValue(ExpandCommandParameterProperty);
            command.Execute(commandParameter);
        }
    }
}
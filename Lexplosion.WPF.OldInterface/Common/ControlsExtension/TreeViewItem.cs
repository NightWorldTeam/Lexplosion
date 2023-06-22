using System.Windows;
using System.Windows.Input;

namespace Lexplosion.Common.Extension
{
    public static class TreeViewItem
    {
        public static DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(TreeViewItem),
                new UIPropertyMetadata(OnCommandChanged));

        public static DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "CommandParameter",
                typeof(object),
                typeof(TreeViewItem),
                new UIPropertyMetadata(null));

        public static void SetCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(CommandProperty, value);
        }

        public static ICommand GetCommand(DependencyObject d, ICommand value)
        {
            return (ICommand)d.GetValue(CommandProperty);
        }

        public static void SetCommandParameter(DependencyObject target, object value)
        {
            target.SetValue(CommandParameterProperty, value);
        }

        public static object GetCommandParameter(DependencyObject target)
        {
            return target.GetValue(CommandParameterProperty);
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
            ICommand command = (ICommand)control.GetValue(CommandProperty);
            object commandParameter = control.GetValue(CommandParameterProperty);
            command.Execute(commandParameter);
        }
    }
}

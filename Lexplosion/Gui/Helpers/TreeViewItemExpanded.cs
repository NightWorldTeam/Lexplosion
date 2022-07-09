using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lexplosion.Gui.Helpers
{
    public class TreeViewItemExpanded
    {
        public static DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(TreeViewItemExpanded),
                new UIPropertyMetadata(OnCommandChanged));
        
        public static DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "CommandParameter",
                typeof(object),
                typeof(TreeViewItemExpanded),
                new UIPropertyMetadata(null));

        public static void SetCommand(DependencyObject target, ICommand value) 
        {
            target.SetValue(CommandProperty, value);
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
            TreeViewItem control = d as TreeViewItem;

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
            TreeViewItem control = sender as TreeViewItem;
            ICommand command = (ICommand)control.GetValue(CommandProperty);
            object commandParameter = control.GetValue(CommandParameterProperty);
            command.Execute(commandParameter);
        }
    }
}

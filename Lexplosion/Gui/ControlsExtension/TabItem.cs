using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lexplosion.Gui.Extension
{
    public class TabItem
    {
        public static readonly DependencyProperty CommandProperty
            = DependencyProperty.Register(
                "Command",
                typeof(ICommand),
                typeof(System.Windows.Controls.TabItem),
                new FrameworkPropertyMetadata(new RelayCommand(obj => {}))
                );

        public static void SetCommand(DependencyObject obj, Action value) 
        {
            obj.SetValue(CommandProperty, value);
        }

        public static Action GetCommand(DependencyObject obj) 
        {
            return (Action)obj.GetValue(CommandProperty);
        }
    }
}

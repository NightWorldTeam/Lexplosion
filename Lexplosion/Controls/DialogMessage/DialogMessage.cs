using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Lexplosion.Controls
{
    public class DialogMessage : ToastMessage
    {
        public static readonly DependencyProperty LeftButtonCommandProperty
            = DependencyProperty.Register("LeftButtonCommand", typeof(string), typeof(DialogMessage), new PropertyMetadata());

        public static readonly DependencyProperty RightButtonCommandProperty
            = DependencyProperty.Register("RightButtonCommand", typeof(string), typeof(DialogMessage), new PropertyMetadata());

        public static readonly DependencyProperty LeftButtonContentProperty
            = DependencyProperty.Register("LeftButtonContent", typeof(string), typeof(DialogMessage), new PropertyMetadata());

        public static readonly DependencyProperty RightButtonContentProperty
            = DependencyProperty.Register("RightButtonContent", typeof(string), typeof(DialogMessage), new PropertyMetadata());

        public string LeftButtonCommand
        {
            get => (string)GetValue(LeftButtonCommandProperty);
            set => SetValue(LeftButtonCommandProperty, value);
        }

        public string RightButtonCommand
        {
            get => (string)GetValue(RightButtonCommandProperty);
            set => SetValue(RightButtonCommandProperty, value);
        }

        public string LeftButtonContent
        {
            get => (string)GetValue(LeftButtonContentProperty);
            set => SetValue(LeftButtonContentProperty, value);
        }

        public string RightButtonContent
        {
            get => (string)GetValue(RightButtonContentProperty);
            set => SetValue(RightButtonContentProperty, value);
        }
    }
}

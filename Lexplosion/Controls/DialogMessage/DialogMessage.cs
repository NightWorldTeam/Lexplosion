using System.Windows;
using System.Windows.Input;

namespace Lexplosion.Controls
{
    public enum DialogResult 
    {
        Undefined,
        Yes,
        No
    }

    public class DialogMessage : ToastMessage
    {
        public static readonly DependencyProperty LeftButtonCommandProperty
            = DependencyProperty.Register("LeftButtonCommand", typeof(ICommand), typeof(DialogMessage), new PropertyMetadata());

        public static readonly DependencyProperty RightButtonCommandProperty
            = DependencyProperty.Register("RightButtonCommand", typeof(ICommand), typeof(DialogMessage), new PropertyMetadata());

        public static readonly DependencyProperty LeftButtonContentProperty
            = DependencyProperty.Register("LeftButtonContent", typeof(string), typeof(DialogMessage), new PropertyMetadata("Yes"));

        public static readonly DependencyProperty RightButtonContentProperty
            = DependencyProperty.Register("RightButtonContent", typeof(string), typeof(DialogMessage), new PropertyMetadata("No"));

        public ICommand LeftButtonCommand
        {
            get => (ICommand)GetValue(LeftButtonCommandProperty);
            set => SetValue(LeftButtonCommandProperty, value);
        }

        public ICommand RightButtonCommand
        {
            get => (ICommand)GetValue(RightButtonCommandProperty);
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

        static DialogMessage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogMessage), new FrameworkPropertyMetadata(typeof(DialogMessage)));
        }
    }
}

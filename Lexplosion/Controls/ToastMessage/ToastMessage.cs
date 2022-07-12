using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lexplosion.Controls
{

    public enum ToastMessageState
    {
        Notification,
        Error
    }

    public class ToastMessage : ContentControl
    {
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(ToastMessage), new PropertyMetadata());

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(ToastMessage), new PropertyMetadata());

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(ToastMessageState), typeof(ToastMessage), new PropertyMetadata(ToastMessageState.Notification   ));

        public static readonly DependencyProperty CloseCommandProperty = 
            DependencyProperty.Register("CloseCommand", typeof(ICommand), typeof(ToastMessage), new PropertyMetadata());

        public string Header 
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public string Message 
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public ToastMessageState State 
        {
            get => (ToastMessageState)GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        public ICommand CloseCommand 
        {
            get => (ICommand)GetValue(CloseCommandProperty);
            set => SetValue(CloseCommandProperty, value);
        }

        static ToastMessage() 
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToastMessage), new FrameworkPropertyMetadata(typeof(ToastMessage)));
        }
    }
}

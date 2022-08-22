using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Lexplosion.Controls
{

    public enum ToastMessageState
    {
        Notification,
        Error
    }

    public class ToastMessage : ContentControl
    {
        #region DependencyProperty Register


        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(ToastMessage), new PropertyMetadata());

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(ToastMessage), new PropertyMetadata());

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(ToastMessageState), typeof(ToastMessage), new PropertyMetadata(ToastMessageState.Notification));

        public static readonly DependencyProperty CloseCommandProperty = 
            DependencyProperty.Register("CloseCommand", typeof(ICommand), typeof(ToastMessage), new PropertyMetadata());

        public static readonly DependencyProperty VisibilityTimeProperty =
            DependencyProperty.Register("VisibilityTime", typeof(int), typeof(ToastMessage), new PropertyMetadata(-1));

        
        #endregion DependencyProperty Register


        #region getters / settes
        
        
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

        public int VisibilityTime 
        {
            get => (int)GetValue(VisibilityTimeProperty);
            set => SetValue(VisibilityTimeProperty, value);
        }


        #endregion getters / setters


        #region constructors

        static ToastMessage() 
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToastMessage), new FrameworkPropertyMetadata(typeof(ToastMessage)));
        }

        public ToastMessage()
        {
            // here we call timer.
            // TODO: Сделать анимацию.

            if (VisibilityTime != -1) 
            { 
                Lexplosion.Run.TaskRun(() => 
                {
                    Thread.Sleep(VisibilityTime);

                    App.Current.Dispatcher.Invoke(() => 
                    { 
                        CloseCommand.Execute(null);
                    });
                });
            }
        }

        #endregion constructors
    }
}

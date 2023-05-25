using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Lexplosion.Controls
{

    public enum ToastMessageState : byte
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
            DependencyProperty.Register("VisibilityTime", typeof(TimeSpan?), typeof(ToastMessage), new PropertyMetadata(null, OnVisibilityTimeChanged));

        private static void OnVisibilityTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as ToastMessage;
            if (e.NewValue == null && e.OldValue == null)
                return;

            if (e.NewValue != null)
            {
                var newValue = (TimeSpan)e.NewValue;
                if (newValue.TotalMilliseconds != TimeSpan.MaxValue.TotalMilliseconds)
                {
                    Runtime.TaskRun(() =>
                    {
                        Thread.Sleep((Int32)newValue.TotalMilliseconds);

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            var CollabsedAnimation = new DoubleAnimation()
                            {
                                From = 1.0,
                                To = 0.0,
                                Duration = TimeSpan.FromSeconds(0.4)
                            };
                            CollabsedAnimation.Completed += (object sender, EventArgs e) =>
                            {
                                obj.CloseCommand.Execute(null);
                            };
                            obj.BeginAnimation(FrameworkElement.OpacityProperty, CollabsedAnimation);
                        });
                    });
                }
            }
        }

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

        public TimeSpan? VisibilityTime
        {
            get => (TimeSpan?)GetValue(VisibilityTimeProperty);
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
            // here we call _timer.
            // TODO: Сделать анимацию.
        }

        #endregion constructors

    }
}

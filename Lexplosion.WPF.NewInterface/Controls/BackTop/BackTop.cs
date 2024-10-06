using Lexplosion.WPF.NewInterface.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Lexplosion.WPF.NewInterface.Controls
{
    internal class BackTop : AdvancedButton
    {
        private double _scrollValue = 0;


        #region Properties


        public static readonly DependencyProperty TargetScrollProperty =
           DependencyProperty.Register(nameof(TargetScroll), typeof(ScrollViewer), typeof(BackTop),
               new FrameworkPropertyMetadata(null, OnTargetScrollChanged));

        public static readonly DependencyProperty ShowFromProperty =
           DependencyProperty.Register(nameof(ShowFrom), typeof(double), typeof(BackTop),
               new FrameworkPropertyMetadata(96.0d));

        public ScrollViewer TargetScroll
        {
            get => (ScrollViewer)GetValue(TargetScrollProperty);
            set => SetValue(TargetScrollProperty, value);
        }

        public double ShowFrom
        {
            get => (double)GetValue(TargetScrollProperty);
            set => SetValue(TargetScrollProperty, value);
        }


        #endregion Properties


        #region Private Methods


        protected override void OnClick()
        {
            ScrollViewerExtensions.ScroollToPosAnimated(
                TargetScroll, 
                ScrollViewerExtensions.GetScrollBar(TargetScroll).Minimum);

            base.OnClick();
        }


        private void TargetScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            const double animationTime = 0.3;

            var viewer = (ScrollViewer)sender;
            _scrollValue = viewer.VerticalOffset;

            if (viewer.VerticalOffset >= 96)
            {
                if (Visibility != Visibility.Visible)
                {
                    Visibility = Visibility.Visible;

                    DoubleAnimation doubleAnimation = new DoubleAnimation()
                    {
                        From = 0.0,
                        To = 1.0,
                        Duration = TimeSpan.FromSeconds(animationTime),
                        EasingFunction = new SineEase()
                        {
                            EasingMode = EasingMode.EaseIn
                        }
                    };

                    ThicknessAnimation thicknessAnimation = new ThicknessAnimation()
                    {
                        From = new Thickness(0, 0, 20, -20),
                        To = new Thickness(0, 0, 20, 20),
                        Duration = TimeSpan.FromSeconds(animationTime),
                        EasingFunction = new SineEase()
                        {
                            EasingMode = EasingMode.EaseIn
                        }
                    };

                    BeginAnimation(FrameworkElement.MarginProperty, thicknessAnimation);
                    BeginAnimation(FrameworkElement.OpacityProperty, doubleAnimation);
                }
            }
            else
            {
                if (Visibility == Visibility.Visible)
                {
                    DoubleAnimation doubleAnimation = new DoubleAnimation()
                    {
                        From = 1.0,
                        To = 0.0,
                        Duration = TimeSpan.FromSeconds(animationTime),
                        EasingFunction = new SineEase()
                        {
                            EasingMode = EasingMode.EaseOut
                        }
                    };

                    ThicknessAnimation thicknessAnimation = new ThicknessAnimation()
                    {
                        From = new Thickness(0, 0, 20, 20),
                        To = new Thickness(0, 0, 20, -20),
                        Duration = TimeSpan.FromSeconds(animationTime),
                        EasingFunction = new SineEase()
                        {
                            EasingMode = EasingMode.EaseOut
                        }
                    };

                    thicknessAnimation.Completed += delegate (object sender, EventArgs e)
                    {
                        Visibility = Visibility.Collapsed;
                    };

                    BeginAnimation(FrameworkElement.MarginProperty, thicknessAnimation);
                    BeginAnimation(FrameworkElement.OpacityProperty, doubleAnimation);
                }
            }

            try
            {
                var onScrollCommand = ScrollViewerExtensions.GetOnScrollCommand(viewer);
                onScrollCommand.Execute(null);
            }
            catch
            {

            }
        }


        private static void OnTargetScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BackTop _this)
            {
                (e.NewValue as ScrollViewer).ScrollChanged += _this.TargetScroll_ScrollChanged;
            }
        }


        #endregion Private Methods
    }
}

using Lexplosion.UI.WPF.Extensions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Lexplosion.UI.WPF.Controls
{
    public class BackTop : Button
    {
        private double _scrollValue = 0;


        #region Properties


        public static readonly DependencyProperty TargetScrollProperty =
           DependencyProperty.Register(nameof(TargetScroll), typeof(ScrollViewer), typeof(BackTop),
               new FrameworkPropertyMetadata(null, OnTargetScrollChanged));

        public static readonly DependencyProperty ShowFromProperty =
           DependencyProperty.Register(nameof(ShowFrom), typeof(double), typeof(BackTop),
               new FrameworkPropertyMetadata(96.0d));

        public static readonly DependencyProperty IconDataProperty =
            DependencyProperty.Register(nameof(IconData), typeof(string), typeof(BackTop),
                new FrameworkPropertyMetadata(defaultValue: string.Empty, propertyChangedCallback: OnIconDataChanged));

        /***  Corner Radius  ***/

        public static readonly DependencyProperty CornerRadiusProperty
            = DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(BackTop),
                new FrameworkPropertyMetadata(new CornerRadius(),
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender),
                    new ValidateValueCallback(IsCornerRadiusValid));

        public static readonly DependencyProperty ToMarginProperty =
           DependencyProperty.Register(nameof(ToMargin), typeof(double), typeof(BackTop),
                new FrameworkPropertyMetadata(defaultValue: 0.0d));

        public static readonly DependencyProperty FromMarginProperty =
            DependencyProperty.Register(nameof(FromMargin), typeof(double), typeof(BackTop),
                new FrameworkPropertyMetadata(defaultValue: -20.0d));

        public static readonly DependencyProperty TopMarginProperty =
            DependencyProperty.Register(nameof(TopMargin), typeof(double), typeof(BackTop),
                new FrameworkPropertyMetadata(defaultValue: 10.0d));

        public double TopMargin
        {
            get => (double)GetValue(TopMarginProperty);
            set => SetValue(TopMarginProperty, value);
        }

        public double ToMargin
        {
            get => (double)GetValue(ToMarginProperty);
            set => SetValue(ToMarginProperty, value);
        }

        public double FromMargin
        {
            get => (double)GetValue(FromMarginProperty);
            set => SetValue(FromMarginProperty, value);
        }

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public string IconData
        {
            get => (string)GetValue(IconDataProperty);
            set => SetValue(IconDataProperty, value);
        }

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
                        From = new Thickness(0, TopMargin, 0, FromMargin),
                        To = new Thickness(0, TopMargin, 0, ToMargin),
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
                        From = new Thickness(0, TopMargin, 0, ToMargin),
                        To = new Thickness(0, TopMargin, 0, FromMargin),
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


        private static void OnIconDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private static bool IsCornerRadiusValid(object value)
        {
            CornerRadius cr = (CornerRadius)value;
            return cr.IsValid(false, false, false, false);
        }

        #endregion Private Methods
    }
}

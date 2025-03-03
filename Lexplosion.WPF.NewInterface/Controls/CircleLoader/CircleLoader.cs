using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;

namespace Lexplosion.WPF.NewInterface.Controls
{
    public class CircleLoader : Control
    {
        public static readonly DependencyProperty IsActiveProperty =
                   DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(CircleLoader), 
                       new FrameworkPropertyMetadata(false, OnIsActiveChanged));

        public static readonly DependencyProperty CornerRadiusProperty
            = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(CircleLoader),
                new FrameworkPropertyMetadata(new CornerRadius(),
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender),
                    new ValidateValueCallback(IsCornerRadiusValid));

        public static readonly DependencyProperty BlurTargetProperty
            = DependencyProperty.Register("BlurTarget", typeof(UIElement), typeof(CircleLoader),
                new FrameworkPropertyMetadata(propertyChangedCallback: OnBlurTargetChanged));

        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.Register("IconWidth", typeof(double), typeof(CircleLoader),
                        new FrameworkPropertyMetadata(Double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure,
                                new PropertyChangedCallback(OnIconWidthChanged)), new ValidateValueCallback(IsWidthHeightValid));

        public static readonly DependencyProperty IconHeightProperty =
            DependencyProperty.Register("IconHeight", typeof(double), typeof(CircleLoader),
                new FrameworkPropertyMetadata(Double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure,
                        new PropertyChangedCallback(OnIconHeightChanged)), new ValidateValueCallback(IsWidthHeightValid));

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public UIElement BlurTarget
        {
            get => (UIElement)GetValue(BlurTargetProperty);
            set => SetValue(BlurTargetProperty, value);
        }

        public double IconWidth
        {
            get => (double)GetValue(IconWidthProperty);
            set => SetValue(IconWidthProperty, value);
        }

        public double IconHeight
        {
            get => (double)GetValue(IconHeightProperty);
            set => SetValue(IconHeightProperty, value);
        }


        #region Contructors


        static CircleLoader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CircleLoader), new FrameworkPropertyMetadata(typeof(CircleLoader)));
        }


        #endregion Constructors


        #region Public & Protected Methods


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        protected virtual void OnIconHeightChanged() { }

        protected virtual void OnIconWidthChanged() { }


        #endregion Public & Protected Methods


        #region Private Methods


        private void SetBlurToTarget()
        {
            if (BlurTarget != null)
            {
                BlurTarget.Effect = IsActive ? new BlurEffect() : null;
                //Console.WriteLine($"OnBlurTargetChanged {BlurTarget.Effect}");
            }
        }

        private static bool IsWidthHeightValid(object value)
        {
            double v = (double)value;
            return double.IsNaN(v) || (v >= 0.0d && !double.IsPositiveInfinity(v));
        }

        private static bool IsCornerRadiusValid(object value)
        {
            CornerRadius cr = (CornerRadius)value;
            return true;//cr.IsValid(false, false, false, false);
        }

        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CircleLoader _this)
            {
                _this.SetBlurToTarget();
                //Console.WriteLine("OnIsActiveChanged");
            }
        }

        private static void OnBlurTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CircleLoader _this)
            {
                if (e.OldValue is UIElement blurTarget)
                    blurTarget.Effect = null;
                _this.SetBlurToTarget();
            }
        }


        private static void OnIconWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CircleLoader _this) 
            {
                _this.OnIconWidthChanged();
            }
        }

        private static void OnIconHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CircleLoader _this)
            {
                _this.OnIconHeightChanged();
            }
        }


        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            Runtime.DebugWrite("Circle Loader OnRenderSizeChanged");
            if (sizeInfo.WidthChanged) 
            {
                IconWidth = sizeInfo.NewSize.Width * 0.5625;
                IconHeight = sizeInfo.NewSize.Height * 0.5625;
            }

            base.OnRenderSizeChanged(sizeInfo);
        }


        #endregion Private Methods
    }
}

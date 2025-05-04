using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Lexplosion.WPF.NewInterface.Controls
{
    [TemplatePart(Name = PART_BACKGROUND_LAYER, Type = typeof(Border))]
    //[TemplatePart(Name = PART_CONTENT_BORDER, Type = typeof(Border))]
    //[TemplatePart(Name = PART_RECTANGLE, Type = typeof(Rectangle))]
    //[TemplatePart(Name = PART_PLACEHOLDER, Type = typeof(TextBlock))]
    public class LoadingBoard : ContentControl
    {
        private const string PART_BACKGROUND_LAYER = "PART_Backround_Layer";
        //private const string PART_CONTENT_BORDER = "PART_Content_Border";
        //private const string PART_RECTANGLE = "PART_Rectangle";
        //private const string PART_PLACEHOLDER = "PART_Placeholder";

        #region Properties and Events


        public static readonly DependencyProperty IsLoadingFinishedProperty
            = DependencyProperty.Register("IsActive", typeof(bool), typeof(LoadingBoard), new PropertyMetadata(false, propertyChangedCallback: OnIsActiveChanged));

        public static readonly DependencyProperty PlaceholderProperty
            = DependencyProperty.Register("Placeholder", typeof(string), typeof(LoadingBoard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty RectangeColorProperty
            = DependencyProperty.Register("RectangeColor", typeof(Brush), typeof(LoadingBoard), new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty BorderColorProperty
            = DependencyProperty.Register("BorderColor", typeof(Brush), typeof(LoadingBoard), new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty BackgroundOpacityProperty
            = DependencyProperty.Register("BackgroundOpacity", typeof(double), typeof(LoadingBoard), new FrameworkPropertyMetadata(1.0, propertyChangedCallback: OnBackgroundOpacityChanged));

        public static readonly DependencyProperty CornerRadiusProperty
            = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(LoadingBoard),
                                  new FrameworkPropertyMetadata(new CornerRadius(),
                                  FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender),
                                  new ValidateValueCallback(IsCornerRadiusValid));

        public static readonly DependencyProperty BlurTargetProperty
            = DependencyProperty.Register("BlurTarget", typeof(UIElement), typeof(LoadingBoard), 
                new FrameworkPropertyMetadata(propertyChangedCallback: OnBlurTargetChanged));

        public static readonly DependencyProperty PlaceholderKeyProperty
            = DependencyProperty.Register("PlaceholderKey", typeof(string), typeof(LoadingBoard), 
                new PropertyMetadata(string.Empty, propertyChangedCallback: OnPlaceholderKeyChanged));

        public bool IsActive
        {
            get => (bool)GetValue(IsLoadingFinishedProperty);
            set => SetValue(IsLoadingFinishedProperty, value);
        }

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public string PlaceholderKey 
        {
            get => (string)GetValue(PlaceholderKeyProperty);
            set => SetValue(PlaceholderKeyProperty, value);
        }

        public Brush RectangeColor
        {
            get => (Brush)GetValue(RectangeColorProperty);
            set => SetValue(RectangeColorProperty, value);
        }

        public Brush BorderColor
        {
            get => (Brush)GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }

        public double BackgroundOpacity
        {
            get => (double)GetValue(BackgroundOpacityProperty);
            set => SetValue(BackgroundOpacityProperty, value);
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


        #endregion


        #region Constructors


        static LoadingBoard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingBoard), new FrameworkPropertyMetadata(typeof(LoadingBoard)));
        }


        #endregion Constructors


        #region Public & Protected Methods


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void SetBlurToTarget() 
        {
            if (BlurTarget != null) {  
                BlurTarget.Effect = IsActive ? new BlurEffect() : null;
            }
        }


        private static bool IsCornerRadiusValid(object value)
        {
            CornerRadius cr = (CornerRadius)value;
            return cr.IsValid(false, false, false, false);
        }

        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LoadingBoard _this)
            {
                _this.SetBlurToTarget();
            }
        }

        private static void OnBlurTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LoadingBoard _this)
            {
                if (e.OldValue is UIElement blurTarget)
                    blurTarget.Effect = null;
                _this.SetBlurToTarget();
            }
        }

        private static void OnBackgroundOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LoadingBoard _this)
            {
                var scBursh = new SolidColorBrush((_this.Background as SolidColorBrush).Color);
                scBursh.Opacity = (double)e.NewValue;

                _this.Background = scBursh;
            }
        }

        private static void OnPlaceholderKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LoadingBoard _this) 
            {
                _this.Placeholder = App.Current.Resources[_this.PlaceholderKey] as string;
            }
        }

        #endregion Private Methods
    }
}

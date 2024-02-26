using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.Controls
{
    [TemplatePart(Name = PART_BACKGROUND_LAYER, Type = typeof(Grid))]
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
            = DependencyProperty.Register("IsActive", typeof(bool), typeof(LoadingBoard), new PropertyMetadata(false));

        public static readonly DependencyProperty PlaceholderProperty
            = DependencyProperty.Register("Placeholder", typeof(string), typeof(LoadingBoard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty RectangeColorProperty
            = DependencyProperty.Register("RectangeColor", typeof(Brush), typeof(LoadingBoard), new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty BorderColorProperty
            = DependencyProperty.Register("BorderColor", typeof(Brush), typeof(LoadingBoard), new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty BackgroundColorProperty
            = DependencyProperty.Register("BackgroundColor", typeof(Color), typeof(LoadingBoard), new FrameworkPropertyMetadata(Colors.Transparent));

        public static readonly DependencyProperty BackgroundOpacityProperty
            = DependencyProperty.Register("BackgroundOpacity", typeof(double), typeof(LoadingBoard), new FrameworkPropertyMetadata(1.0));

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

        public Color BackgroundColor
        {
            get => (Color)GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }

        public double BackgroundOpacity
        {
            get => (double)GetValue(BackgroundOpacityProperty);
            set => SetValue(BackgroundOpacityProperty, value);
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
    }
}

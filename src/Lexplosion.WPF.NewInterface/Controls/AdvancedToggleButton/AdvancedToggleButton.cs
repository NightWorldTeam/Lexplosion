using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Lexplosion.WPF.NewInterface.Controls
{
    public class AdvancedToggleButton : ToggleButton
    {
        private const string PART_ICON_NAME = "PATH_Icon";
        private const string PART_TEXT_NAME = "PATH_Text";

        private Viewbox _iconViewBox;
        private Path _iconPath;
        private TextBlock _textBlock;


        #region Properties


        public static readonly DependencyProperty TextProperty
            = DependencyProperty.Register(nameof(Text), typeof(string), typeof(AdvancedToggleButton),
                new FrameworkPropertyMetadata(defaultValue: string.Empty, propertyChangedCallback: OnTextChanged));

        public static readonly DependencyProperty IconDataProperty
            = DependencyProperty.Register(nameof(IconData), typeof(string), typeof(AdvancedToggleButton),
                new FrameworkPropertyMetadata(defaultValue: string.Empty, propertyChangedCallback: OnIconDataChanged));

        /***  Padding  ***/

        public static readonly DependencyProperty IconPaddingProperty
            = DependencyProperty.Register(nameof(IconPadding), typeof(Thickness), typeof(AdvancedToggleButton),
                new FrameworkPropertyMetadata(new Thickness(), FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        public static readonly DependencyProperty TextPaddingProperty
            = DependencyProperty.Register(nameof(TextPadding), typeof(Thickness), typeof(AdvancedToggleButton),
            new FrameworkPropertyMetadata(new Thickness(), FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        /***  Corner Radius  ***/

        public static readonly DependencyProperty CornerRadiusProperty
            = DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(AdvancedToggleButton),
                new FrameworkPropertyMetadata(new CornerRadius(),
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender),
                    new ValidateValueCallback(IsCornerRadiusValid));

        /*** Icon Width/Height ***/

        public static readonly DependencyProperty IconWidthProperty
            = DependencyProperty.Register(nameof(IconWidth), typeof(double), typeof(AdvancedToggleButton),
            new FrameworkPropertyMetadata(Double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure), 
            new ValidateValueCallback(IsWidthHeightValid));

        public static readonly DependencyProperty IconHeightProperty
            = DependencyProperty.Register(nameof(IconHeight), typeof(double), typeof(AdvancedToggleButton),
            new FrameworkPropertyMetadata(Double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure),
            new ValidateValueCallback(IsWidthHeightValid));

        /*** Icon Color ***/

        public static readonly DependencyProperty IconFillProperty
            = DependencyProperty.Register(nameof(IconFill), typeof(Brush), typeof(AdvancedToggleButton),
                new FrameworkPropertyMetadata(SystemColors.ControlTextBrush, FrameworkPropertyMetadataOptions.Inherits, propertyChangedCallback: OnIconFillChanged));

        private static void OnIconFillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AdvancedToggleButton _this)
            {
                if (_this.IconFill == null)
                {
                    _this._iconPath?.SetValue(Path.FillProperty, new TemplateBindingExtension(AdvancedToggleButton.ForegroundProperty));
                }
                else
                {
                    if (_this._iconPath != null) 
                    {
                        _this._iconPath.SetValue(Path.FillProperty, new TemplateBindingExtension(AdvancedToggleButton.IconFillProperty));
                        _this._iconPath.Fill = _this.IconFill;
                    }
                }
            }
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string IconData
        {
            get => (string)GetValue(IconDataProperty);
            set => SetValue(IconDataProperty, value);
        }

        public Thickness IconPadding
        {
            get => (Thickness)GetValue(IconPaddingProperty);
            set => SetValue(IconPaddingProperty, value);
        }

        public Thickness TextPadding
        {
            get => (Thickness)GetValue(TextPaddingProperty);
            set => SetValue(TextPaddingProperty, value);
        }

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
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

        public Brush IconFill 
        {
            get => (Brush)GetValue(IconFillProperty);
            set => SetValue(IconFillProperty, value);
        }


        #endregion Properties


        #region Constructors


        static AdvancedToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AdvancedToggleButton), new FrameworkPropertyMetadata(typeof(AdvancedToggleButton)));
        }


        public AdvancedToggleButton() : base()
        {
        }


        #endregion Constructors


        #region Public & Protected Methods


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _iconViewBox = Template.FindName(PART_ICON_NAME, this) as Viewbox;

            if (_iconViewBox != null)
            {
                _iconPath = _iconViewBox.GetChildOfType<Path>();
                _iconPath.Data = Geometry.Parse(IconData);
            }

            _textBlock = Template.FindName(PART_TEXT_NAME, this) as TextBlock;

            if (_textBlock != null)
            {
                _textBlock.Text = Text;
            }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            // Если контент был присвоен через свойство Content, вырубаем Icon и TextBlock.
            ChangeAdvancedContentVisibility(newContent == null);
        }


        #endregion Public & Protected Methods


        #region Private Methods


        /// <summary>
        /// Включает или выключает продвинутый контент (Icon[ViewBox+Path], TextBlock).
        /// </summary>
        /// <param name="isEnable">Включить/Выключить</param>
        private void ChangeAdvancedContentVisibility(bool isEnable)
        {
            if (isEnable)
            {
                _iconViewBox.Visibility = Visibility.Visible;
                _textBlock.Visibility = Visibility.Visible;
                return;
            }

            _iconViewBox.Visibility = Visibility.Collapsed;
            _textBlock.Visibility = Visibility.Collapsed;
        }


        private static void OnIconDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AdvancedToggleButton _this)
            {
                if (_this._iconPath != null)
                {
                    if (string.IsNullOrEmpty(_this.IconData))
                    {
                        _this._iconViewBox.Visibility = Visibility.Collapsed;
                        return;
                    }

                    _this._iconViewBox.Visibility = Visibility.Visible;
                    _this._iconPath.Data = Geometry.Parse(_this.IconData);
                }
            }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AdvancedToggleButton _this)
            {
                if (_this._textBlock != null)
                    _this._textBlock.Text = _this.Text;
            }
        }

        private static bool IsCornerRadiusValid(object value)
        {
            CornerRadius cr = (CornerRadius)value;
            return cr.IsValid(false, false, false, false);
        }


        #endregion Private Methods


        private static bool IsWidthHeightValid(object value)
        {
            double v = (double)value;
            return (Double.IsNaN(v)) || (v >= 0.0d && !Double.IsPositiveInfinity(v));
        }

        private static void OnTransformDirty(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Callback for MinWidth, MaxWidth, Width, MinHeight, MaxHeight, Height, and RenderTransformOffset
            //FrameworkElement fe = (FrameworkElement)d;
            //fe.AreTransformsClean = false;
        }
    }
}

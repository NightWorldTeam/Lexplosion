using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Lexplosion.WPF.NewInterface.Controls
{
    public class AdvancedButton : Button
    {
        //
        //
        // Icon [x] 
        // Text [x]
        // 
        //

        private const string PART_ICON_NAME = "PATH_Icon";
        private const string PART_TEXT_NAME = "PATH_Text";

        private Viewbox _iconViewBox;
        private Path _iconPath;
        private TextBlock _textBlock;


        #region Properties


        public static readonly DependencyProperty TextProperty
            = DependencyProperty.Register(nameof(Text), typeof(string), typeof(AdvancedButton),
                new FrameworkPropertyMetadata(defaultValue: string.Empty, propertyChangedCallback: OnTextChanged));

        public static readonly DependencyProperty IconDataProperty
            = DependencyProperty.Register(nameof(IconData), typeof(string), typeof(AdvancedButton),
                new FrameworkPropertyMetadata(defaultValue: string.Empty, propertyChangedCallback: OnIconDataChanged));

        /***  Padding  ***/

        public static readonly DependencyProperty IconPaddingProperty
            = DependencyProperty.Register(nameof(IconPadding), typeof(Thickness), typeof(AdvancedButton),
                new FrameworkPropertyMetadata(new Thickness(), FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        public static readonly DependencyProperty TextPaddingProperty
            = DependencyProperty.Register(nameof(TextPadding), typeof(Thickness), typeof(AdvancedButton),
            new FrameworkPropertyMetadata(new Thickness(), FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        /***  Corner Radius  ***/

        public static readonly DependencyProperty CornerRadiusProperty
            = DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(AdvancedButton),
                new FrameworkPropertyMetadata(new CornerRadius(), 
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender),
                    new ValidateValueCallback(IsCornerRadiusValid));


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


        #endregion Properties


        #region Constructors


        static AdvancedButton() 
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AdvancedButton), new FrameworkPropertyMetadata(typeof(AdvancedButton)));
        }


        public AdvancedButton() : base()
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
                //if (_iconPath != null)
                //{
                //    if (string.IsNullOrEmpty(IconData))
                //    {
                //        _iconViewBox.Visibility = Visibility.Collapsed;
                //        return;
                //    }

                //    _iconViewBox.Visibility = Visibility.Visible;
                //    _iconPath.Data = Geometry.Parse(IconData);
                //}

                _iconPath.Data = Geometry.Parse(IconData);
            }

            _textBlock = Template.FindName(PART_TEXT_NAME, this) as TextBlock;

            if (_textBlock != null) 
            {
                _textBlock.Text = Text;   
            }
        }


        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
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
            if (d is AdvancedButton _this)
            {
                Runtime.DebugWrite("Icon data changed");
                if (_this._iconPath != null) 
                {
                    if (string.IsNullOrEmpty(_this.IconData)) { 
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
            if (d is AdvancedButton _this)
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
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Lexplosion.UI.WPF.Controls
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
        private Grid _loadingContent;
        private Grid _mainContent;


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

        /*** Icon Color ***/

        public static readonly DependencyProperty IconFillProperty
            = DependencyProperty.Register(nameof(IconFill), typeof(Brush), typeof(AdvancedButton),
                new FrameworkPropertyMetadata(SystemColors.ControlTextBrush, FrameworkPropertyMetadataOptions.Inherits, propertyChangedCallback: OnIconFillChanged));

        private static readonly DependencyPropertyKey HasIconPropertyKey
            = DependencyProperty.RegisterReadOnly(nameof(HasIcon), typeof(bool), typeof(AdvancedButton),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty HasIconProperty = HasIconPropertyKey.DependencyProperty;


        /*** Loading ***/


        public static readonly DependencyProperty IsLoadingProperty
            = DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(AdvancedButton),
            new FrameworkPropertyMetadata(false, propertyChangedCallback: OnIsLoadingPropertyChanged));

        public static readonly DependencyProperty LoadingTextProperty
            = DependencyProperty.Register(nameof(LoadingText), typeof(string), typeof(AdvancedButton),
            new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty LoadingActionProperty
            = DependencyProperty.Register(nameof(LoadingAction), typeof(ICommand), typeof(AdvancedButton),
            new FrameworkPropertyMetadata(null, propertyChangedCallback: OnIsLoadingPropertyChanged));

        /// <summary>
        /// Идет ли загрузка
        /// </summary>
        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public string LoadingText
        {
            get => (string)GetValue(LoadingTextProperty);
            set => SetValue(LoadingTextProperty, value);
        }

        public ICommand LoadingAction
        {
            get => (ICommand)GetValue(LoadingActionProperty);
            set => SetValue(LoadingActionProperty, value);
        }

        private void OnIsLoadingPropertyChanged(bool newValue)
        {
            //if ((bool)newValue)
            //{
            //    _loadingContent.Visibility = Visibility.Visible;
            //    _mainContent.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    _loadingContent.Visibility = Visibility.Collapsed;
            //    _mainContent.Visibility = Visibility.Visible;
            //}
        }


        private static void OnIsLoadingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AdvancedButton _this)
            {
                if (_this._loadingContent == null || _this._mainContent == null)
                    return;

                _this.OnIsLoadingPropertyChanged((bool)e.NewValue);
            }
        }














        public bool HasIcon
        {
            get => (bool)GetValue(HasIconProperty);
            protected set => SetValue(HasIconPropertyKey, value);
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

        public Brush IconFill
        {
            get => (Brush)GetValue(IconFillProperty);
            set => SetValue(IconFillProperty, value);
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
            _loadingContent = Template.FindName("PART_LoadingContent", this) as Grid;
            _mainContent = Template.FindName("DefaultContent", this) as Grid;

            if (_textBlock != null)
            {
                _textBlock.Text = Text;
            }

            HasIcon = IconData != null;

            OnIsLoadingPropertyChanged(IsLoading);
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
                if (_iconViewBox != null)
                {
                    _iconViewBox.Visibility = Visibility.Visible;
                }

                if (_textBlock != null)
                {
                    _textBlock.Visibility = Visibility.Visible;
                }
                return;
            }

            if (_iconViewBox != null)
            {
                _iconViewBox.Visibility = Visibility.Collapsed;
            }

            if (_textBlock != null)
            {
                _textBlock.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Иконка изменилась
        /// </summary>
        private static void OnIconDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AdvancedButton _this)
            {
                //Runtime.DebugWrite("Icon data changed");
                if (_this._iconPath != null)
                {
                    if (string.IsNullOrEmpty(_this.IconData))
                    {
                        _this._iconViewBox.Visibility = Visibility.Collapsed;
                        _this.HasIcon = false;
                        return;
                    }

                    _this._iconViewBox.Visibility = Visibility.Visible;
                    _this._iconPath.Data = Geometry.Parse(_this.IconData);
                    _this.HasIcon = true;
                }
            }
        }

        /// <summary>
        /// Текст изменился
        /// </summary>
        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AdvancedButton _this)
            {
                if (_this._textBlock == null)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(_this._textBlock.Text))
                {
                    _this._textBlock.Visibility = Visibility.Collapsed;
                    return;
                }

                _this._textBlock.Visibility = Visibility.Visible;
                _this._textBlock.Text = _this.Text;
            }
        }

        private static bool IsCornerRadiusValid(object value)
        {
            CornerRadius cr = (CornerRadius)value;
            return cr.IsValid(false, false, false, false);
        }

        /// <summary>
        /// Цвет иконки изменился
        /// </summary>
        private static void OnIconFillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AdvancedButton _this)
            {
                if (_this._iconPath == null)
                {
                    return;
                }

                if (_this.IconFill == null)
                {
                    _this._iconPath.SetValue(Path.FillProperty, new TemplateBindingExtension(AdvancedButton.ForegroundProperty));
                }
                else
                {
                    _this._iconPath.SetValue(Path.FillProperty, new TemplateBindingExtension(AdvancedButton.IconFillProperty));
                    _this._iconPath.Fill = _this.IconFill;
                }
            }
        }

        protected override void OnClick()
        {
            if (IsLoading)
            {
                LoadingAction?.Execute(null);
                return;
            }

            base.OnClick();
        }

        #endregion Private Methods
    }
}

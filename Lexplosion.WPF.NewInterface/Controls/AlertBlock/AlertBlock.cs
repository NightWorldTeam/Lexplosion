using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Lexplosion.WPF.NewInterface.Controls
{
    public enum AlertBlockType 
    {
        Information,
        Important,
        Warning,
        Error
    }

    public class AlertBlock : ContentControl
    {
        private const string PART_ICON_NAME = "PATH_Icon";
        private const string PART_TITLE_NAME = "PATH_Title";
        private const string PART_TEXT_NAME = "PATH_Text";

        private Viewbox _iconViewBox;
        private Path _iconPath;
        private TextBlock _textBlock;
        private TextBlock _titleTextBlock;


        #region Properties


        public static readonly DependencyProperty IconDataProperties
            = DependencyProperty.Register(nameof(IconData), typeof(string), typeof(AlertBlock),
                new FrameworkPropertyMetadata(defaultValue: string.Empty, propertyChangedCallback: OnIconDataChanged));

        public static readonly DependencyProperty TitleProperties
            = DependencyProperty.Register(nameof(Title), typeof(string), typeof(AlertBlock),
            new FrameworkPropertyMetadata(defaultValue:  string.Empty, propertyChangedCallback: OnTitleChanged));

        public static readonly DependencyProperty TextProperties
            = DependencyProperty.Register(nameof(Text), typeof(string), typeof(AlertBlock),
            new FrameworkPropertyMetadata(defaultValue: string.Empty, propertyChangedCallback: OnTextChanged));

        public string Title 
        {
            get => (string)GetValue(TitleProperties);
            set => SetValue(TitleProperties, value);
        }

        public string Text 
        {
            get => (string)GetValue(TextProperties);
            set => SetValue(TextProperties, value);
        }

        public string IconData 
        {
            get => (string)GetValue(IconDataProperties);
            set => SetValue(IconDataProperties, value);
        }


        #endregion Properties


        #region Constructors


        static AlertBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AlertBlock), new FrameworkPropertyMetadata(typeof(AlertBlock)));
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

            _titleTextBlock = Template.FindName(PART_TITLE_NAME, this) as TextBlock;

            if (_titleTextBlock != null)
            {
                _titleTextBlock.Text = Title;
            }
        }


        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
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
            if (d is AlertBlock _this)
            {
                Runtime.DebugWrite("Icon data changed");
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
            if (d is AlertBlock _this)
            {
                if (_this._textBlock != null)
                    _this._textBlock.Text = _this.Text;
            }
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }


        #endregion Private Methods
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Lexplosion.WPF.NewInterface.Controls
{
    [TemplatePart(Name = PART_ICON_VIEWBOX_NAME, Type = typeof(Viewbox))]
    [TemplatePart(Name = PART_ICON_NAME, Type = typeof(Path))]
    [TemplatePart(Name = PART_PLACEHOLDER_NAME, Type = typeof(TextBlock))]
    public class AdvancedTextBox : TextBox
    {
        private const string PART_ICON_NAME = "PART_Icon";
        private const string PART_ICON_VIEWBOX_NAME = "PART_IconViewBox";
        private const string PART_PLACEHOLDER_NAME = "PART_Placeholder";

        private Viewbox _viewBox;
        private Path _path;
        private TextBlock _placeholder;


        #region Dependency Properties


        public static readonly DependencyProperty IconKeyProperty
            = DependencyProperty.Register(nameof(IconKey), typeof(string), typeof(AdvancedTextBox),
                new FrameworkPropertyMetadata(defaultValue: string.Empty, propertyChangedCallback: OnIsIconKeyChanged));

        public static readonly DependencyProperty PlaceholderKeyProperty
            = DependencyProperty.Register(nameof(PlaceholderKey), typeof(string), typeof(AdvancedTextBox),
                new FrameworkPropertyMetadata(defaultValue: string.Empty, propertyChangedCallback: OnPlaceholderKeyChanged));


        /// ----- Readonly Properties ----- ///


        private static readonly DependencyPropertyKey IsEmptyPropertyKey
            = DependencyProperty.RegisterReadOnly(nameof(IsEmpty), typeof(bool), typeof(AdvancedTextBox),
                new FrameworkPropertyMetadata(true));

        private static readonly DependencyProperty IsEmptyProperty = IsEmptyPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsIconEmptyPropertyKey
            = DependencyProperty.RegisterReadOnly(nameof(IsIconKeyEmpty), typeof(bool), typeof(AdvancedTextBox),
                new FrameworkPropertyMetadata(defaultValue: false));

        private static readonly DependencyProperty IsIconEmptyProperty = IsIconEmptyPropertyKey.DependencyProperty;

        public string IconKey
        {
            get => (string)GetValue(IconKeyProperty);
            set => SetValue(IconKeyProperty, value);
        }

        public string PlaceholderKey
        {
            get => (string)GetValue(PlaceholderKeyProperty);
            set => SetValue(PlaceholderKeyProperty, value);
        }

        public bool IsEmpty
        {
            get => (bool)GetValue(IsEmptyProperty);
            private set => SetValue(IsEmptyPropertyKey, value);
        }

        public bool IsIconKeyEmpty
        {
            get => (bool)GetValue(IsIconEmptyProperty);
            private set => SetValue(IsIconEmptyPropertyKey, value);
        }


        #endregion Dependency Properties


        #region Public & Protected Methods


        public override void OnApplyTemplate()
        {
            _viewBox = Template.FindName(PART_ICON_VIEWBOX_NAME, this) as Viewbox;
            _path = Template.FindName(PART_ICON_NAME, this) as Path;
            _placeholder = Template.FindName(PART_PLACEHOLDER_NAME, this) as TextBlock;

            UpdateIsEmpty();
            IsIconKeyEmpty = string.IsNullOrEmpty(IconKey);

            UpdatePath(IconKey);
            UpdatePlaceholder(PlaceholderKey);

            base.OnApplyTemplate();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            UpdateIsEmpty();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            UpdateIsEmpty();
            base.OnLostFocus(e);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            UpdateIsEmpty();
            base.OnTextChanged(e);
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void UpdateIsEmpty()
        {
            IsEmpty = string.IsNullOrEmpty(Text);
            _placeholder.Visibility = IsEmpty ? Visibility.Visible : Visibility.Collapsed;
        }

        private static void OnIsIconKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = d as AdvancedTextBox;
            instance.UpdatePath(e.NewValue);
        }

        private static void OnPlaceholderKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = d as AdvancedTextBox;
            instance.UpdatePlaceholder(e.NewValue);
        }


        private void UpdatePath(object newValue)
        {
            if (_path == null || newValue == null) return;

            if (string.IsNullOrEmpty((string)newValue))
            {
                _viewBox.Visibility = Visibility.Collapsed;
                _path.Visibility = Visibility.Collapsed;
                return;
            }

            _path.Visibility = Visibility;
            _viewBox.Visibility = Visibility.Visible;
            // DP + Key -> Data ThemesResourcePath Key
            _path.Data = Geometry.Parse((string)App.Current.Resources["PD" + newValue]);
        }

        private void UpdatePlaceholder(object newValue)
        {
            if (_placeholder == null) return;

            if (string.IsNullOrEmpty((string)newValue))
            {
                _placeholder.Visibility = Visibility.Collapsed;
                return;
            }

            _placeholder.Visibility = Visibility.Visible;
            _placeholder.SetResourceReference(System.Windows.Controls.TextBlock.TextProperty, newValue);
        }


        #endregion Private Methods
    }
}

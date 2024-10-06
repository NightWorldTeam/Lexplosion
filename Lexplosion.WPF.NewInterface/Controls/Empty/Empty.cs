using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Controls
{
    public class Empty : ContentControl
    {
        public static readonly DependencyProperty DescriptionProperty
            = DependencyProperty.Register(nameof(Description), typeof(string), typeof(Empty),
            new FrameworkPropertyMetadata(defaultValue: "No data"));

        public static readonly DependencyProperty CollectionCountProperty
            = DependencyProperty.Register(nameof(CollectionCount), typeof(int), typeof(Empty),
            new FrameworkPropertyMetadata(defaultValue: 0, propertyChangedCallback: OnCollectionCountChanged));

        public static readonly DependencyProperty IsContentLoadingProperty
            = DependencyProperty.Register(nameof(IsContentLoading), typeof(bool), typeof(Empty),
            new FrameworkPropertyMetadata(defaultValue: false, propertyChangedCallback: OnIsContentLoadingChanged));

        public string Description 
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public int CollectionCount 
        {
            get => (int)GetValue(CollectionCountProperty);
            set => SetValue(CollectionCountProperty, value);
        }

        public bool IsContentLoading
        {
            get => (bool)GetValue(CollectionCountProperty);
            set => SetValue(CollectionCountProperty, value);
        }


        #region Constructors


        static Empty()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Empty), new FrameworkPropertyMetadata(typeof(Empty)));
        }


        #endregion Constructors


        private static void OnCollectionCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = d as Empty;
            _this.Visibility = _this.CollectionCount == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private static void OnIsContentLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = d as Empty;
            _this.Visibility = _this.CollectionCount == 0 && !((bool)e.NewValue) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}

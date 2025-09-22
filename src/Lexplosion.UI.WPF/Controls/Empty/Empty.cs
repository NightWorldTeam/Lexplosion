using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lexplosion.UI.WPF.Controls
{
    [TemplatePart(Name = PART_Title, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_Description, Type = typeof(TextBlock))]
    public class Empty : ContentControl
    {
        private const string PART_Title = "Title";
        private const string PART_Description = "Description";

        private TextBlock _titleTextBlock;
        private TextBlock _descriptionTextBlock;


        public static readonly DependencyProperty DescriptionProperty
            = DependencyProperty.Register(nameof(Description), typeof(string), typeof(Empty),
            new FrameworkPropertyMetadata(defaultValue: "No data"));

        public static readonly DependencyProperty CollectionCountProperty
            = DependencyProperty.Register(nameof(CollectionCount), typeof(int), typeof(Empty),
            new FrameworkPropertyMetadata(defaultValue: 0, propertyChangedCallback: OnCollectionCountChanged));

        public static readonly DependencyProperty IsContentLoadingProperty
            = DependencyProperty.Register(nameof(IsContentLoading), typeof(bool), typeof(Empty),
            new FrameworkPropertyMetadata(defaultValue: false, propertyChangedCallback: OnIsContentLoadingChanged));

        public static readonly DependencyProperty TitleProperty
            = DependencyProperty.Register(nameof(Title), typeof(string), typeof(Empty),
                new FrameworkPropertyMetadata(propertyChangedCallback: OnTitleChanged));

        public static readonly DependencyProperty TitleForegroundProperty
            = DependencyProperty.Register(nameof(TitleForeground), typeof(Brush), typeof(Empty));

        public static readonly DependencyProperty DescriptionMaxWidthProperty
            = DependencyProperty.Register(nameof(DescriptionMaxWidth), typeof(double), typeof(Empty),
                new FrameworkPropertyMetadata(defaultValue: 400d, FrameworkPropertyMetadataOptions.AffectsMeasure));

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

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public double DescriptionMaxWidth
        {
            get => (double)GetValue(DescriptionMaxWidthProperty);
            set => SetValue(DescriptionMaxWidthProperty, value);
        }

        public Brush TitleForeground
        {
            get => (Brush)GetValue(TitleForegroundProperty);
            set => SetValue(TitleForegroundProperty, value);
        }


        #region Constructors


        static Empty()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Empty), new FrameworkPropertyMetadata(typeof(Empty)));
        }

        public override void OnApplyTemplate()
        {
            _titleTextBlock = Template.FindName(PART_Title, this) as TextBlock;
            _descriptionTextBlock = Template.FindName(PART_Description, this) as TextBlock;
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

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = (d as Empty);

            if (_this._titleTextBlock == null)
            {
                return;
            }

            var newValue = e.NewValue as string;

            if (newValue == null)
            {
                _this._titleTextBlock.Visibility = Visibility.Collapsed;
                return;
            }


            _this._titleTextBlock.Visibility = Visibility.Collapsed;
            _this.Title = e.NewValue as string;
        }
    }
}

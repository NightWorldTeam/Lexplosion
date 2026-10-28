using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Lexplosion.UI.WPF.Core;

namespace Lexplosion.UI.WPF.Controls
{
    [TemplatePart(Name = PART_LINE_NAME, Type = typeof(Border))]
    [TemplatePart(Name = PART_HEADER_NAME, Type = typeof(Border))]
    public class UIUnderlineTabControl : TabControl
    {
        private const string PART_LINE_NAME = "PART_Line";
        private const string PART_HEADER_NAME = "PART_Header";

        private Border _line;
        private Border _header;
        private TranslateTransform _linePosition;
        private object _requestedContent;
        private bool _contentPending;
        private int _selectionVersion;

        private TabItem _selectedTabItem;
        private bool _underlineUpdatePending;
        private int _previousSelectedIndex = -1;

        #region Dependency Properties

        // Set to false for pages hosting HWND controls (for example WebView2).
        // Existing uses retain their animated underline and content by default.
        public static readonly DependencyProperty EnableAnimationsProperty =
            DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(UIUnderlineTabControl),
                new FrameworkPropertyMetadata(true));

        public bool EnableAnimations
        {
            get => (bool)GetValue(EnableAnimationsProperty);
            set => SetValue(EnableAnimationsProperty, value);
        }

        // Allows disabling only the content slide while preserving the underline motion.
        public static readonly DependencyProperty ContentAnimationProperty =
            DependencyProperty.Register(nameof(ContentAnimation), typeof(ContentControlAnimation),
                typeof(UIUnderlineTabControl),
                new FrameworkPropertyMetadata(ContentControlAnimation.SlideToLeft,
                    (d, e) => ((UIUnderlineTabControl)d).UpdateContentDirection(false)));

        public ContentControlAnimation ContentAnimation
        {
            get => (ContentControlAnimation)GetValue(ContentAnimationProperty);
            set => SetValue(ContentAnimationProperty, value);
        }

        // The XAML template uses the resolved direction, leaving ContentAnimation
        // as the caller's preferred forward direction.
        private static readonly DependencyPropertyKey EffectiveContentAnimationPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(EffectiveContentAnimation),
                typeof(ContentControlAnimation), typeof(UIUnderlineTabControl),
                new FrameworkPropertyMetadata(ContentControlAnimation.SlideToLeft));

        public static readonly DependencyProperty EffectiveContentAnimationProperty =
            EffectiveContentAnimationPropertyKey.DependencyProperty;

        public ContentControlAnimation EffectiveContentAnimation =>
            (ContentControlAnimation)GetValue(EffectiveContentAnimationProperty);

        // SelectedContent is updated by TabControl during selection. Keep the
        // displayed page separate so the header can finish moving before heavy
        // content is materialized on the WPF UI thread.
        private static readonly DependencyPropertyKey DisplayedContentPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(DisplayedContent), typeof(object),
                typeof(UIUnderlineTabControl), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty DisplayedContentProperty =
            DisplayedContentPropertyKey.DependencyProperty;

        public object DisplayedContent => GetValue(DisplayedContentProperty);

        public static readonly DependencyProperty TabPanelPaddingProperty
            = DependencyProperty.Register("TabPanelPadding", typeof(Thickness), typeof(UIUnderlineTabControl),
                new FrameworkPropertyMetadata(
                    defaultValue: new Thickness(),
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure,
                    propertyChangedCallback: OnTabPanelPadding));

        public Thickness TabPanelPadding
        {
            get => (Thickness)GetValue(TabPanelPaddingProperty);
            set => SetValue(TabPanelPaddingProperty, value);
        }


        #endregion Dependency Properties


        #region Constructors


        static UIUnderlineTabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UIUnderlineTabControl), new FrameworkPropertyMetadata(typeof(UIUnderlineTabControl)));
        }

        public UIUnderlineTabControl()
        {
            Loaded += (sender, e) =>
            {
                // A preselected item may have received its content after
                // OnApplyTemplate without raising another selection event.
                if (DisplayedContent == null)
                    SetValue(DisplayedContentPropertyKey, SelectedContent);
                QueueUnderlineUpdate();
            };
            ItemContainerGenerator.StatusChanged += (sender, e) =>
            {
                if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                    QueueUnderlineUpdate();
            };
        }


        #endregion Constructors


        #region Public & Protected Methods


        protected override void OnInitialized(EventArgs e)
        {
            //Runtime.DebugWrite(nameof(OnInitialized));
            base.OnInitialized(e);
        }

        public override void OnApplyTemplate()
        {
            if (_selectedTabItem != null)
                _selectedTabItem.SizeChanged -= OnTabItemSizeChanged;

            base.OnApplyTemplate();

            _line = Template.FindName(PART_LINE_NAME, this) as Border;
            _linePosition = _line?.RenderTransform as TranslateTransform;
            if (_linePosition?.IsFrozen == true)
            {
                _linePosition = (TranslateTransform)_linePosition.CloneCurrentValue();
                _line.RenderTransform = _linePosition;
            }
            _header = Template.FindName(PART_HEADER_NAME, this) as Border;
            if (_header != null)
                _header.Padding = TabPanelPadding;

            // TabItem widths become valid after the template's initial layout pass.
            if (DisplayedContent == null)
                SetValue(DisplayedContentPropertyKey, SelectedContent);
            QueueUnderlineUpdate();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            // Update BEFORE base: TabControl synchronizes SelectedContent in base,
            // so AnimatedContentControl must see the new direction before its content changes.
            if (ReferenceEquals(e.OriginalSource, this) &&
                SelectedIndex >= 0 && SelectedIndex != _previousSelectedIndex)
            {
                bool backwards = _previousSelectedIndex >= 0 && SelectedIndex < _previousSelectedIndex;
                UpdateContentDirection(backwards);
                _previousSelectedIndex = SelectedIndex;
            }

            base.OnSelectionChanged(e);

            if (ReferenceEquals(e.OriginalSource, this))
                QueueContentNavigation();

            // Selection can change while ItemContainerGenerator is still generating.
            QueueUnderlineUpdate();
        }

        private void UpdateContentDirection(bool backwards)
        {
            var direction = ContentAnimation;
            if (backwards)
            {
                switch (direction)
                {
                    case ContentControlAnimation.SlideToLeft:
                        direction = ContentControlAnimation.SlideToRight;
                        break;
                    case ContentControlAnimation.SlideToRight:
                        direction = ContentControlAnimation.SlideToLeft;
                        break;
                    case ContentControlAnimation.SlideToTop:
                        direction = ContentControlAnimation.SlideToBottom;
                        break;
                    case ContentControlAnimation.SlideToBottom:
                        direction = ContentControlAnimation.SlideToTop;
                        break;
                }
            }

            SetValue(EffectiveContentAnimationPropertyKey, direction);
        }

        private void QueueContentNavigation()
        {
            _requestedContent = SelectedContent;
            ++_selectionVersion;

            if (ReferenceEquals(DisplayedContent, _requestedContent))
            {
                _contentPending = false;
                return;
            }

            _contentPending = true;
            if (!IsLoaded || _linePosition == null || !EnableAnimations ||
                !AnimationPreferences.Enabled || DisplayedContent == null)
                CommitContentNavigation(false);
        }

        private void CommitContentNavigation(bool afterAnimation)
        {
            if (!_contentPending) return;

            int version = _selectionVersion;
            var content = _requestedContent;
            _contentPending = false;

            if (!afterAnimation)
            {
                SetValue(DisplayedContentPropertyKey, content);
                return;
            }

            // The underline's final frame is rendered before the incoming
            // DataTemplate starts its potentially expensive construction.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (version == _selectionVersion)
                    SetValue(DisplayedContentPropertyKey, content);
            }), DispatcherPriority.Background);
        }

        private void QueueUnderlineUpdate()
        {
            if (_underlineUpdatePending) return;
            _underlineUpdatePending = true;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _underlineUpdatePending = false;
                if (IsLoaded && ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    if (SelectedIndex < 0 && Items.Count > 0)
                        SelectedIndex = 0;

                    UpdateUnderline();
                }
            }), DispatcherPriority.Loaded);
        }

        private void UpdateUnderline()
        {
            if (_selectedTabItem != null)
                _selectedTabItem.SizeChanged -= OnTabItemSizeChanged;

            // During initialization or a collection reset there may be no selected tab.
            // ContainerFromIndex(-1) can throw inside WPF's RealizedItemBlock.ContainerAt.
            if (SelectedIndex < 0 || SelectedIndex >= Items.Count)
            {
                CommitContentNavigation(false);
                return;
            }

            _selectedTabItem = ItemContainerGenerator.ContainerFromIndex(SelectedIndex) as TabItem
                               ?? SelectedItem as TabItem;
            if (_selectedTabItem == null || _line == null)
            {
                CommitContentNavigation(false);
                return;
            }

            _selectedTabItem.SizeChanged += OnTabItemSizeChanged;
            var padding = GetTemplateBorderPadding(_selectedTabItem);
            SetUnderlineWidth(Math.Max(0, _selectedTabItem.ActualWidth - padding.Left - padding.Right));
            MoveLine(padding.Left);
        }

        #endregion Public & Protected Methods


        #region Private Methods


        /// <summary>
        /// Вызывается при изменении размера TabItem.
        /// </summary>
        private void OnTabItemSizeChanged(object sender, SizeChangedEventArgs e)
        {
            QueueUnderlineUpdate();
        }


        /// <summary>
        /// Перемещает линию позиционируя её центрально относительно выбранной вкладки
        /// </summary>
        private void MoveLine(double tabPaddingLeft)
        {
            var sumWidth = tabPaddingLeft;
            var iterCount = 0;
            foreach (var obj in ItemContainerGenerator.Items)
            {
                FrameworkElement element;
                if (!(obj is FrameworkElement))
                {
                    element = ItemContainerGenerator.ContainerFromItem(obj) as FrameworkElement;
                }
                else
                {
                    element = obj as FrameworkElement;
                }

                if (iterCount == SelectedIndex)
                {
                    break;
                }

                if (element != null) sumWidth += element.ActualWidth;
                iterCount++;
            }

            SetUnderlineOffset(sumWidth);
        }

        private static void OnTabPanelPadding(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue) return;

            var instance = (UIUnderlineTabControl)d;
            if (instance._header != null)
                instance._header.Padding = (Thickness)e.NewValue;
        }

        private static Thickness GetTemplateBorderPadding(FrameworkElement element)
        {
            if (element is TabItem)
            {
                var tabItem = element as TabItem;
                var border = tabItem.Template?.FindName("Border", tabItem) as Border;

                if (border != null)
                {
                    return border.Padding;
                }
                else
                {
                    Runtime.DebugWrite("Border for tabitem not found");
                }
            }
            return new Thickness(0);
        }

        private void SetUnderlineOffset(double left)
        {
            if (_linePosition == null)
            {
                CommitContentNavigation(false);
                return;
            }

            if (!EnableAnimations || !AnimationPreferences.Enabled || !IsLoaded)
            {
                _linePosition.BeginAnimation(TranslateTransform.XProperty, null);
                _linePosition.X = left;
                CommitContentNavigation(false);
                return;
            }

            // RenderTransform is compositor-friendly; animating Margin forces
            // a layout pass every frame and competes with page construction.
            double current = _linePosition.X;
            if (Math.Abs(current - left) < 0.5)
            {
                CommitContentNavigation(true);
                return;
            }

            int version = _selectionVersion;
            var position = _linePosition;
            var motion = new DoubleAnimation(current, left, TimeSpan.FromMilliseconds(200))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            motion.Completed += (sender, args) =>
            {
                if (version != _selectionVersion || !ReferenceEquals(_linePosition, position))
                    return;

                position.BeginAnimation(TranslateTransform.XProperty, null);
                position.X = left;
                CommitContentNavigation(true);
            };
            position.BeginAnimation(TranslateTransform.XProperty, motion);
        }

        private void SetUnderlineWidth(double width)
        {
            if (_line == null) return;
            if (!EnableAnimations || !AnimationPreferences.Enabled)
            {
                _line.BeginAnimation(WidthProperty, null);
                _line.Width = width;
                return;
            }
            if (width <= 0) return;
            _line.BeginAnimation(WidthProperty, new DoubleAnimation
            {
                To = width,
                Duration = TimeSpan.FromMilliseconds(200)
            });
        }

        #endregion Private Methods
    }
}

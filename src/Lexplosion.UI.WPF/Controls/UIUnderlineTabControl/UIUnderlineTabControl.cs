using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Lexplosion.UI.WPF.Core;
#if DEBUG
using Lexplosion.QA;
#endif

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
        private int _scheduledContentVersion = -1;

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
#if DEBUG
            using var trace = UiDebugTrace.Measure("tabs.apply-template", 3);
#endif
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

#if DEBUG
            using (UiDebugTrace.Measure("tabs.selection", 3))
#endif
                base.OnSelectionChanged(e);

            if (ReferenceEquals(e.OriginalSource, this))
                QueueContentNavigation();

            // Selection can change while ItemContainerGenerator is still generating.
            QueueUnderlineUpdate();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            // The old template bound directly to SelectedContent. Since the
            // delayed-content refactor, a change after OnSelectionChanged
            // (e.g. late item content or template realization) could be lost.
            // Observe the actual DP as well as the tab selection.
            if (e.Property == SelectedContentProperty &&
                !ReferenceEquals(e.OldValue, e.NewValue))
            {
                QueueContentNavigation();
                // Late-arriving SelectedContent must also reach the existing
                // Loaded-priority underline/commit path, even without a new click.
                QueueUnderlineUpdate();
            }
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
            var requested = SelectedContent;

            if (ReferenceEquals(DisplayedContent, requested))
            {
                // Cancel a queued destination when the user returns to what
                // is already visible, without unnecessarily rebuilding a page.
                if (_contentPending)
                    ++_selectionVersion;
                _contentPending = false;
                _requestedContent = requested;
                return;
            }

            // OnPropertyChanged(SelectedContent) and OnSelectionChanged often
            // report the same selection. Only the first may schedule a commit.
            if (_contentPending && ReferenceEquals(_requestedContent, requested))
                return;

            _requestedContent = requested;
            ++_selectionVersion;
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

            if (!afterAnimation)
            {
                _contentPending = false;
                ApplyDisplayedContent(content);
                return;
            }

            // Keep the request pending until it actually reaches the display.
            // A superseded Dispatcher callback must not silently consume it.
            if (_scheduledContentVersion == version) return;
            _scheduledContentVersion = version;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_scheduledContentVersion == version)
                    _scheduledContentVersion = -1;
                if (version != _selectionVersion || !_contentPending)
                    return;

                _contentPending = false;
                ApplyDisplayedContent(content);
            }), DispatcherPriority.Background);
        }

        private void ApplyDisplayedContent(object content)
        {
#if DEBUG
            UiDebugTrace.Navigate(content);
            if (PerformanceTelemetry.Enabled && content != null)
                PerformanceTelemetry.Record("navigation.tabs." + content.GetType().Name, 0);
            using (UiDebugTrace.Measure("tabs.commit", 3))
#endif
                SetValue(DisplayedContentPropertyKey, content);

#if DEBUG
            if (PerformanceTelemetry.Enabled && !ReferenceEquals(DisplayedContent, SelectedContent))
                PerformanceTelemetry.Record("navigation.tabs.content-mismatch", 0);
#endif
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
#if DEBUG
            using (UiDebugTrace.Measure("tabs.underline", 3))
#endif
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
                _linePosition.BeginAnimation(TranslateTransform.XProperty, null);
                _linePosition.X = left;
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
            };
            position.BeginAnimation(TranslateTransform.XProperty, motion);
            // Start page preparation after the next render opportunity,
            // not after the full 200 ms underline animation.
            CommitContentNavigation(true);
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

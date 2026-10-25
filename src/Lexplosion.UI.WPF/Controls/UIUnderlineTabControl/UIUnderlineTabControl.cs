using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Threading;

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

        private TabItem _selectedTabItem;
        private bool _underlineUpdatePending;

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
            Loaded += (sender, e) => QueueUnderlineUpdate();
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
            _header = Template.FindName(PART_HEADER_NAME, this) as Border;
            if (_header != null)
                _header.Padding = TabPanelPadding;

            // TabItem widths become valid after the template's initial layout pass.
            QueueUnderlineUpdate();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            // Selection can change while ItemContainerGenerator is still generating.
            QueueUnderlineUpdate();
        }

        private void QueueUnderlineUpdate()
        {
            if (_underlineUpdatePending) return;
            _underlineUpdatePending = true;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _underlineUpdatePending = false;
                if (IsLoaded && ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                    UpdateUnderline();
            }), DispatcherPriority.Loaded);
        }

        private void UpdateUnderline()
        {
            if (_selectedTabItem != null)
                _selectedTabItem.SizeChanged -= OnTabItemSizeChanged;

            // During initialization or a collection reset there may be no selected tab.
            // ContainerFromIndex(-1) can throw inside WPF's RealizedItemBlock.ContainerAt.
            if (SelectedIndex < 0 || SelectedIndex >= Items.Count)
                return;

            _selectedTabItem = ItemContainerGenerator.ContainerFromIndex(SelectedIndex) as TabItem
                               ?? SelectedItem as TabItem;
            if (_selectedTabItem == null || _line == null)
                return;

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
            if (_line == null) return;
            var margin = new Thickness(left, 0, 0, 0);
            if (!EnableAnimations)
            {
                _line.BeginAnimation(MarginProperty, null);
                _line.Margin = margin;
                return;
            }
            _line.BeginAnimation(MarginProperty, new ThicknessAnimation
            {
                To = margin,
                Duration = TimeSpan.FromMilliseconds(200)
            });
        }

        private void SetUnderlineWidth(double width)
        {
            if (_line == null) return;
            if (!EnableAnimations)
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

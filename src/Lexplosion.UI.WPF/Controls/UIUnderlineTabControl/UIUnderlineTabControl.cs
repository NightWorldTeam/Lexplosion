using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lexplosion.UI.WPF.Controls
{
    [TemplatePart(Name = PART_LINE_NAME, Type = typeof(Border))]
    [TemplatePart(Name = PART_HEADER_NAME, Type = typeof(Border))]
    public class UIUnderlineTabControl : TabControl
    {
        private const string PART_LINE_NAME = "PART_Line";
        private const string PART_HEADER_NAME = "PART_Header";

        private Border _line;
        private TabPanel _tabPanel;
        private Border _header;
        private ContentPresenter _contentPresenter;

        private TabItem _selectedTabItem;

        private Thickness _loadedTabPanelPadding = new Thickness();

        #region Dependency Properties


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
            Loaded += UIUnderlineTabControl_Loaded;
        }

        private void UIUnderlineTabControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Data-bound tabs, such as Friends, need a default selection.
            EnsureSelection();
            if (_line == null || SelectedIndex < 0)
                return;

            var selected = ItemContainerGenerator.ContainerFromItem(SelectedItem) as TabItem
                ?? SelectedItem as TabItem;
            if (_selectedTabItem != null)
                _selectedTabItem.SizeChanged -= OnTabItemSizeChanged;

            _selectedTabItem = selected;
            if (_selectedTabItem == null)
                return;

            // Subscribe even if the item has not been measured yet.
            _selectedTabItem.SizeChanged += OnTabItemSizeChanged;
            if (selected.ActualWidth <= 0)
                return;

            // The first selection occurs before PART_Line is measured. Initialize it
            // after layout, with no animation or Dispatcher queue.
            var padding = GetTemplateBorderPadding(selected);
            _line.BeginAnimation(WidthProperty, null);
            _line.Width = Math.Max(0, selected.ActualWidth - padding.Left - padding.Right);

            var offset = padding.Left;
            for (int i = 0; i < SelectedIndex; i++)
            {
                var item = ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement
                    ?? Items[i] as FrameworkElement;
                offset += item?.ActualWidth ?? 0;
            }

            _line.BeginAnimation(MarginProperty, null);
            _line.Margin = new Thickness(offset, 0, 0, 0);
        }


        #endregion Constructors


        #region Public & Protected Methods


        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            if (IsLoaded)
                EnsureSelection();
        }

        private void EnsureSelection()
        {
            if (SelectedIndex < 0 && Items.Count > 0)
                SelectedIndex = 0;
        }

        public override void OnApplyTemplate()
        {
            _line = Template.FindName(PART_LINE_NAME, this) as Border;
            _tabPanel = Template.FindName("HeaderPanel", this) as TabPanel;
            _header = Template.FindName(PART_HEADER_NAME, this) as Border;
            _selectedTabItem = ItemContainerGenerator.ContainerFromItem(SelectedItem) as TabItem ?? SelectedItem as TabItem;
            
            if (_header != null)
            {
                //Runtime.DebugWrite(_header);
                _header.Padding = _loadedTabPanelPadding;
            }

            if (_line != null)
            {
            }

            base.OnApplyTemplate();
            EnsureSelection();
        }

        //override 

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (_selectedTabItem != null)
                _selectedTabItem.SizeChanged -= OnTabItemSizeChanged;

            _selectedTabItem = ItemContainerGenerator.ContainerFromItem(SelectedItem) as TabItem
                ?? SelectedItem as TabItem;

            if (_selectedTabItem != null)
            {
                _selectedTabItem.SizeChanged += OnTabItemSizeChanged;

                // Selection can change before OnApplyTemplate creates PART_Line.
                // Loaded and SizeChanged position it when the layout is ready.
                if (_line != null && _selectedTabItem.ActualWidth > 0)
                {
                    var padding = GetTemplateBorderPadding(_selectedTabItem);
                    SmoothChangeWidth(_line, _selectedTabItem.ActualWidth - padding.Left - padding.Right);
                    MoveLine(padding.Left);
                }
            }

            base.OnSelectionChanged(e);
        }


        #endregion Public & Protected Methods


        #region Private Methods


        /// <summary>
        /// Вызывается при изменении размера TabItem.
        /// </summary>
        private void OnTabItemSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_line == null || !ReferenceEquals(sender, _selectedTabItem))
                return;

            var tabItem = (TabItem)sender;
            var tabItemPadding = GetTemplateBorderPadding(tabItem);
            SmoothChangeWidth(_line, tabItem.ActualWidth - tabItemPadding.Left - tabItemPadding.Right);
            MoveLine(tabItemPadding.Left);
        }


        /// <summary>
        /// Перемещает линию позиционируя её центрально относительно выбранной вкладки
        /// </summary>
        private void MoveLine(double tabPaddingLeft)
        {
            if (_line == null || SelectedIndex < 0)
                return;

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

                sumWidth += element?.ActualWidth ?? 0;
                iterCount++;
            }

            SmoothMoveLine(_line, sumWidth);
        }

        private static void OnTabPanelPadding(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue) return;

            var instance = (UIUnderlineTabControl)d;
            if (instance._header == null)
            {
                instance._loadedTabPanelPadding = (Thickness)e.NewValue;
                return;
            }

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

        private static void SmoothMoveLine(FrameworkElement target, double to)
        {
            if (target == null) return;

            var moveTranslate = new ThicknessAnimation()
            {
                To = new Thickness(to, 0, 0, 0),
                Duration = new TimeSpan(0, 0, 0, 0, 200),
            };
            target.BeginAnimation(MarginProperty, moveTranslate);

        }

        private static void SmoothChangeWidth(FrameworkElement target, double to)
        {
            if (target == null || to <= 0) return;

            //Runtime.DebugWrite(to);
            var changeWidthTranslate = new DoubleAnimation()
            {
                From = target.ActualWidth,
                To = to,
                Duration = new TimeSpan(0, 0, 0, 0, 200)
            };
            target.BeginAnimation(WidthProperty, changeWidthTranslate);
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject == null)
                yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                var child = VisualTreeHelper.GetChild(dependencyObject, i);
                if (child != null && child is T t)
                    yield return t;

                foreach (var childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }


        #endregion Private Methods
    }
}
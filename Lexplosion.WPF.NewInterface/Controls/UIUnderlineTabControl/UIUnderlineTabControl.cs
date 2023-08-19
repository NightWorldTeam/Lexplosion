using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lexplosion.WPF.NewInterface.Controls
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


        #endregion Constructors


        #region Public & Protected Methods


        protected override void OnInitialized(EventArgs e)
        {
            Runtime.DebugWrite(nameof(OnInitialized));
            base.OnInitialized(e);
        }

        public override void OnApplyTemplate()
        {
            SelectedIndex = 0;
            
            _line = Template.FindName(PART_LINE_NAME, this) as Border;
            _tabPanel = Template.FindName("HeaderPanel", this) as TabPanel;
            _header = Template.FindName(PART_HEADER_NAME, this) as Border;
            //_contentPresenter = Template.FindName("PART_SelectedContentHost", this) as ContentPresenter; 

            if (_header != null) 
            {
                Runtime.DebugWrite(_header);
                _header.Padding = _loadedTabPanelPadding;
            }

            if (_line != null)
            {
            }

            base.OnApplyTemplate();
        }

        //override 

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            try
            {
                var selectedFE = ItemContainerGenerator.ContainerFromItem(SelectedValue) as TabItem;
                var selectedTabPadding = GetTemplateBorderPadding(selectedFE);

                if (selectedFE == null) 
                {
                    return;
                }

                SmoothChangeWidth(_line, selectedFE.ActualWidth - (selectedTabPadding.Left + selectedTabPadding.Right));

                var sumWidth = selectedTabPadding.Left;
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

                    sumWidth += element.ActualWidth;
                    iterCount++;
                }

                SmoothMoveLine(_line, sumWidth);

                var dp = new DoubleAnimation()
                {
                    From = 0.4,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.5)
                };
                //_contentPresenter.BeginAnimation(OpacityProperty, dp);
            }
            catch 
            {
                new Exception("Underline border doesn't exists.");
            }

            base.OnSelectionChanged(e);
        }


        #endregion Public & Protected Methods


        #region Private Methods



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
                var border = tabItem.Template.FindName("Border", tabItem) as Border;

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
            var moveTranslate = new ThicknessAnimation()
            {
                To = new Thickness(to, 0, 0, 0),
                Duration = new TimeSpan(0, 0, 0, 0, 200),
            };
            target.BeginAnimation(MarginProperty, moveTranslate);

        }

        private static void SmoothChangeWidth(FrameworkElement target, double to)
        {
            if (to == 0) return;

            Runtime.DebugWrite(to);
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
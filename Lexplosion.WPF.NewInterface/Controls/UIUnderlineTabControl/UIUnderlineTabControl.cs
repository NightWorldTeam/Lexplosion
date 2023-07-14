using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml.Linq;

namespace Lexplosion.WPF.NewInterface.Controls
{
    [TemplatePart(Name = PART_LINE_NAME, Type = typeof(Border))]
    public class UIUnderlineTabControl : TabControl
    {
        private const string PART_LINE_NAME = "PART_Line";

        private Border _line;
        private TabPanel _tabPanel;


        #region Constructors


        static UIUnderlineTabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UIUnderlineTabControl), new FrameworkPropertyMetadata(typeof(UIUnderlineTabControl)));
        }


        #endregion Constructors


        #region Public & Protected Methods


        public override void OnApplyTemplate()
        {
            _line = Template.FindName(PART_LINE_NAME, this) as Border; 
            _tabPanel = Template.FindName("HeaderPanel", this) as TabPanel;

            if (_line != null) 
            {
                
            }

            base.OnApplyTemplate();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            var selectedFE = ItemContainerGenerator.ContainerFromItem(SelectedValue) as TabItem;
            var selectedTabPadding = GetTemplateBorderPadding(selectedFE);

            SmoothChangeWidth(_line, selectedFE.ActualWidth - selectedTabPadding.Left - selectedTabPadding.Right);

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
            base.OnSelectionChanged(e);
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private Thickness GetTemplateBorderPadding(FrameworkElement element) 
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

        private void SmoothMoveLine(FrameworkElement target, double to) 
        {
            var moveTranslate = new ThicknessAnimation()
            {
                To = new Thickness(to,0,0,0),
                Duration = new TimeSpan(0, 0, 0, 0, 200),
            };
            target.BeginAnimation(MarginProperty, moveTranslate);
        }

        private void SmoothChangeWidth(FrameworkElement target, double to) 
        {
            var changeWidthTranslate = new DoubleAnimation()
            {
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

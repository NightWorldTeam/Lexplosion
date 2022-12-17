using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Lexplosion.Gui.Extension
{
    public static class ScrollViewer
    {
        public static DependencyProperty OnScrollCommandProperty
            = DependencyProperty.RegisterAttached(
                "OnScrollCommand",
                typeof(ICommand),
                typeof(ScrollViewer),
                new FrameworkPropertyMetadata(
                    new RelayCommand(
                        obj => { }
                        )
                    )
                );

        public static readonly DependencyProperty ChildItemsUpdatedProperty
            = DependencyProperty.RegisterAttached(
                "ChildItemsUpdated",
                typeof(bool),
                typeof(ScrollViewer),
                new FrameworkPropertyMetadata(false, OnChildItemsUpdatedChanged));


        public static DependencyProperty VerticalOffsetProperty 
            = DependencyProperty.RegisterAttached(
                "VerticalOffset",
                typeof(double),
                typeof(ScrollViewer),
                new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));

        #region OnScroll Commmand Property


        public static ICommand GetOnScrollCommand(DependencyObject d)
        {
            return (ICommand)d.GetValue(OnScrollCommandProperty);
        }

        public static void SetOnScrollCommand(DependencyObject d, ICommand value)
        {
            d.SetValue(OnScrollCommandProperty, value);
        }


        #endregion OnScroll Commmand Property


        #region ChildItemsCountProperty


        public static bool GetChildItemsUpdated(DependencyObject d)
        {
            return (bool)d.GetValue(ChildItemsUpdatedProperty);
        }

        public static void SetChildItemsUpdated(DependencyObject d, bool value)
        {
            d.SetValue(ChildItemsUpdatedProperty, value);
        }


        #endregion ChildItemsCountProperty


        private static void OnChildItemsUpdatedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = (System.Windows.Controls.ScrollViewer)d;
            var scrollViewerPos = scrollViewer.VerticalOffset;
            var scrollBar = GetScrollBar(scrollViewer);

            ScroollToPosAnimated(scrollViewer, scrollBar.Minimum);
        }

        private static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            System.Windows.Controls.ScrollViewer scrollViewer = (System.Windows.Controls.ScrollViewer)target;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }

        public static void ScroollToPosAnimated(System.Windows.Controls.ScrollViewer control, double toValue) 
        {
            var duration = TimeSpan.FromSeconds(0.2);

            DoubleAnimation animation = new DoubleAnimation()
            {
                From = control.VerticalOffset,
                To = toValue,
                Duration = new Duration(duration)
            };

            control.BeginAnimation(VerticalOffsetProperty, animation);
        }

        public static ScrollBar GetScrollBar(System.Windows.Controls.ScrollViewer viewer) 
        {
            return viewer.Template.FindName("PART_VerticalScrollBar", viewer) as ScrollBar;
        } 
    }
}

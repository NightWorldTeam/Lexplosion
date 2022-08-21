using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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
            scrollViewer.ScrollToTop();
        }
    }
}

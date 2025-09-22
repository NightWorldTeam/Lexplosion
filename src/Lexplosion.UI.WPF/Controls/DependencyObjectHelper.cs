using System.Windows.Media;
using System.Windows;
using System;

namespace Lexplosion.UI.WPF.Controls
{
    public static class DependencyObjectHelper
    {
        public static T FindControl<T>(this DependencyObject parent, Type targetType, string controlName) where T : FrameworkElement
        {

            if (parent == null) 
                return null;

            if (parent.GetType() == targetType && ((T)parent).Name == controlName)
            {
                return (T)parent;
            }

            T result = null;
            int count = VisualTreeHelper.GetChildrenCount(parent);
            
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = (DependencyObject)VisualTreeHelper.GetChild(parent, i);

                if (FindControl<T>(child, targetType, controlName) != null)
                {
                    result = FindControl<T>(child, targetType, controlName);
                    break;
                }
            }
            
            return result;
        }

        public static T GetChildOfType<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return default;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.Controls
{
    public class ChangelogItem : ContentControl
    {
        // Expander [Header]
        // update type ---> aplha, beta, release.
        // version number
        // id
        // Grid [Body]

        public static readonly DependencyProperty UpdateTypeProperty
            = DependencyProperty.Register("UpdateType", typeof(UpdateType), typeof(ChangelogItem));

        public static readonly DependencyProperty VersionProperty
            = DependencyProperty.Register("Version", typeof(string), typeof(ChangelogItem));

        public static readonly DependencyProperty IdProperty
            = DependencyProperty.Register("Id", typeof(int), typeof(ChangelogItem));

        public static readonly DependencyProperty IsExpandedProperty
            = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(ChangelogItem));

        public UpdateType UpdateType 
        {
            get => (UpdateType)GetValue(UpdateTypeProperty);
            set => SetValue(UpdateTypeProperty, value);
        }

        public string Version
        {
            get => (string)GetValue(VersionProperty);
            set => SetValue(VersionProperty, value);
        }

        public int Id
        {
            get => (int)GetValue(IdProperty);
            set => SetValue(IdProperty, value);
        }

        public bool IsExpanded 
        {
            get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        static ChangelogItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChangelogItem), new FrameworkPropertyMetadata(typeof(ChangelogItem)));
        }
    }
}

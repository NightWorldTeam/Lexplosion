using LumiSoft.Net.WebDav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Lexplosion.Controls
{
    public class Filters : ListBox
    {

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(Filters), new PropertyMetadata("Filter by category"));

        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register("Subtitle", typeof(string), typeof(Filters), new PropertyMetadata("128 chips discovered"));

        public static readonly DependencyProperty ItemColorBrushProperty =
            DependencyProperty.Register("ItemColorBrush", typeof(SolidColorBrush), typeof(Filters), new PropertyMetadata(Brushes.Orange));

        public string Title 
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Subtitle 
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public SolidColorBrush ItemColorBrush 
        {
            get => (SolidColorBrush)GetValue(ItemColorBrushProperty);
            set => SetValue(ItemColorBrushProperty, value);
        }



        #region Constructors


        static Filters() 
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Filters), new FrameworkPropertyMetadata(typeof(Filters)));
        }


        #endregion Constructors
    }
}

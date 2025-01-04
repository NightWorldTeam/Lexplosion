﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Documents;

namespace Lexplosion.WPF.NewInterface.Extensions
{
    public static class TextBlockExtensions
    {
        public static readonly DependencyProperty TextByKeyProperty
            = DependencyProperty.RegisterAttached("TextByKey", typeof(string), typeof(TextBlockExtensions),
                new FrameworkPropertyMetadata(string.Empty, OnTextByKeyChanged));

        public static readonly DependencyProperty RunByKeyProperty
            = DependencyProperty.RegisterAttached("RunByKey", typeof(string), typeof(TextBlockExtensions),
            new FrameworkPropertyMetadata(string.Empty, OnRunByKeyChanged));

        public static void SetTextByKey(DependencyObject dp, string value)
        {
            dp.SetValue(TextByKeyProperty, value);
        }

        public static string GetTextByKey(DependencyObject dp)
        {
            return dp.GetValue(TextByKeyProperty) as string;
        }

        private static void OnTextByKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is System.Windows.Controls.TextBlock)
            {
                var textBlock = d as System.Windows.Controls.TextBlock;

                if (e.NewValue == null)
                {
                    Runtime.DebugWrite("Значение ключа null, так быть явно не должно.", color: ConsoleColor.Red);
                    textBlock.Text = "null";
                }

                if (App.Current.Resources[e.NewValue] == null)
                {
                    textBlock.Text = $"Not found {e.NewValue}";
                    return;
                }

                textBlock.SetResourceReference(System.Windows.Controls.TextBlock.TextProperty, e.NewValue);
            }
        }

        public static void SetRunByKey(DependencyObject dp, string value)
        {
            dp.SetValue(TextByKeyProperty, value);
        }

        public static string GetRunByKey(DependencyObject dp)
        {
            return dp.GetValue(TextByKeyProperty) as string;
        }

        private static void OnRunByKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Run)
            {
                var run = d as Run;
                run.SetResourceReference(Run.TextProperty, e.NewValue);
            }
        }
    }
}

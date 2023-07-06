using Lexplosion.WPF.NewInterface.Tools;
using Lexplosion.WPF.NewInterface.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Lexplosion.WPF.NewInterface.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MouseDown += delegate { try { DragMove(); } catch { } };
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            var grid = (Grid)sender;

            for (int i = 0; i < grid.ColumnDefinitions.Count; i++) 
            {
                Runtime.DebugWrite(i.ToString() + " " + grid.ColumnDefinitions[i].ActualWidth.ToString());
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.ChangeColor(ColorTools.GetColorByHex("#167FFC"));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //App.Current.Resources.MergedDictionaries.Clear();

            App.Current.Resources.MergedDictionaries.Clear();

            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Resources/Fonts.xaml")
            });
            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Resources/Icons.xaml")
            });
            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Resources/Styles/TextBlock.xaml")
            });
            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Resources/Styles/Buttons.xaml")
            });
            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Resources/Styles/TextBox.xaml")
            });
            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Resources/Styles/CheckBox.xaml")
            });
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var resourceDictionaries = new List<ResourceDictionary>();

            var currentThemeName = "";

            foreach (var s in App.Current.Resources.MergedDictionaries)
            {
                if (s.Source.ToString().Contains("ColorTheme"))
                {
                    currentThemeName = s.Source.ToString();
                    resourceDictionaries.Add(s);
                }
            }

            foreach (var s in resourceDictionaries)
            {
                App.Current.Resources.MergedDictionaries.Remove(s);
            }

            if (currentThemeName.Contains("Light"))
            {
                App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
                {
                    Source = new Uri("pack://application:,,,/Resources/Themes/DarkColorTheme.xaml")
                });
            }
            else 
            {
                App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
                {
                    Source = new Uri("pack://application:,,,/Resources/Themes/LightColorTheme.xaml")
                });
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var sb = (Storyboard)this.Resources["SizeAnimationSB"];
            sb.Begin(this);
        }
    }
}

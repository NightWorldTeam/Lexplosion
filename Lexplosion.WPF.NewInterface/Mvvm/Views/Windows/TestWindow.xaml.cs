﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;
using static Lexplosion.Logic.Objects.Curseforge.CurseforgeProjectInfo;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        public TestWindow()
        {
            InitializeComponent();

            //hex.TextChanged += Hex_TextChanged;
            //var newColor = Color.FromRgb(19, 242, 135);
            //Console.WriteLine(ColorTools.GetDarkerColor(newColor, 10));
            //Console.WriteLine(ColorTools.GetDarkerColor(newColor, 20));
            //Console.WriteLine(ColorTools.GetDarkerColor(newColor, 70));

            //var list = new List<ConsoleLog>();


            //for (var i = 0; i < 10000; i++) 
            //{
            //    list.Add(new ConsoleLog(RandomString(random.Next(60, 700))));
            //}

            //LogsContainer.ItemsSource = list;
        }

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void Hex_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            //try
            //{
                //var newColor = (Color)ColorConverter.ConvertFromString(hex.Text);
            //    object latestColor = Color.FromRgb(22, 127, 252); // App.Current.Resources["ActivityColor"] ?? ;
            //    var updatedColor = (Color)ColorConverter.ConvertFromString(hex.Text);
            //    var intervalColors = Gradient.GenerateGradient((Color)latestColor, updatedColor, 50); //ColorTools.GetIntervalColor((Color)latestColor, (Color)ColorConverter.ConvertFromString(hex.Text), 50);

            //    Runtime.TaskRun(() =>
            //    { 
            //        var i = 0;
            //        foreach (var newColor in intervalColors) 
            //        {
            //            Console.ForegroundColor = ConsoleColor.Green;
            //            Console.WriteLine($"{i}. {newColor.ToString()}");
            //            App.Current.Dispatcher.Invoke(() => { 
            //                App.Current.Resources["DefaultButtonBackgroundColor"] = newColor;
            //                App.Current.Resources["DefaultButtonBackgroundColorBrush"] = new SolidColorBrush(newColor);
            //            });
            //            App.Current.Resources["HoverAccentColor1"] = ColorTools.GetDarkerColor(newColor, 10);
            //            App.Current.Resources["HoverAccentColor"] = new SolidColorBrush((Color)App.Current.Resources["HoverAccentColor1"]);
            //            App.Current.Resources["PressedAccentColor1"] = ColorTools.GetDarkerColor(newColor, 20);
            //            App.Current.Resources["PressedAccentColor"] = new SolidColorBrush((Color)App.Current.Resources["PressedAccentColor1"]);
            //            App.Current.Resources["DisableAccentColor1"] = ColorTools.GetDarkerColor(newColor, 70);
            //            App.Current.Resources["DisableAccentColor"] = new SolidColorBrush((Color)App.Current.Resources["DisableAccentColor1"]);

            //            App.Current.Resources["ForegroundAccentColor1"] = ColorTools.ForegroundByColor(newColor);
            //            App.Current.Resources["ForegroundAccentColor"] = new SolidColorBrush((Color)App.Current.Resources["ForegroundAccentColor1"]);
            //            Thread.Sleep(10);
            //            i++;
            //        }
            //        Thread.Sleep(10);
            //        App.Current.Dispatcher.Invoke(() => {
            //            App.Current.Resources["DefaultButtonBackgroundColor"] = updatedColor;
            //            App.Current.Resources["DefaultButtonBackgroundColorBrush"] = new SolidColorBrush(updatedColor);
            //        });
            //    });
            //    Console.ForegroundColor = ConsoleColor.White;
            //    Console.WriteLine(App.Current.Resources["AccentColor1"]);
            //}
            //catch (Exception ea) 
            //{
            //    Console.ForegroundColor = ConsoleColor.Red;
            //    Console.WriteLine(ea);
            //    Console.ForegroundColor = ConsoleColor.White;
            //}

            // #13f287
            // #167FFC
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrEmpty(StartTB.Text))
            //    StartTB.Text = "#6501FF";
            //if (string.IsNullOrEmpty(EndTB.Text))
            //    EndTB.Text = "#FEFF01";


            //    var start = (Color)ColorConverter.ConvertFromString(StartTB.Text);
            //var end = (Color)ColorConverter.ConvertFromString(EndTB.Text);

            //Colors.ItemsSource = Gradient.GenerateGradient(start, end, 50);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var da = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(2),
                From = 0,
                To = 1,
                BeginTime = TimeSpan.FromSeconds(0.5)
            };

            da.Completed += Da_Completed;



            // Применяем анимацию к заголовку
            Lexplosion.BeginAnimation(OpacityProperty, da);
            Logo.BeginAnimation(OpacityProperty, da);
        }

        private void Da_Completed(object sender, EventArgs e)
        {
            var da1 = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(1),
                From = 0,
                To = 1,
            };

            da1.Completed += SubtitleLoaded;
            WelcomeText.BeginAnimation(OpacityProperty, da1);
        }

        private void SubtitleLoaded(object sender, EventArgs e)
        {
            var da1 = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(1),
                From = 1,
                To = 0,
                BeginTime = TimeSpan.FromSeconds(2)
            };

            Lexplosion.BeginAnimation(OpacityProperty, da1);
            Logo.BeginAnimation(OpacityProperty, da1);
            WelcomeText.BeginAnimation(OpacityProperty, da1);
        }
    }
}

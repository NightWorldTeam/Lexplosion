using Lexplosion.WPF.NewInterface.NWColorTools;
using Lexplosion.WPF.NewInterface.Tools;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;

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
            
            hex.TextChanged += Hex_TextChanged;
            var newColor = Color.FromRgb(19, 242, 135);
            Console.WriteLine(ColorTools.GetDarkerColor(newColor, 10));
            Console.WriteLine(ColorTools.GetDarkerColor(newColor, 20));
            Console.WriteLine(ColorTools.GetDarkerColor(newColor, 70));
        }

        private void Hex_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                //var newColor = (Color)ColorConverter.ConvertFromString(hex.Text);
                object latestColor = Color.FromRgb(22, 127, 252); // App.Current.Resources["ActivityColor"] ?? ;
                var updatedColor = (Color)ColorConverter.ConvertFromString(hex.Text);
                var intervalColors = Gradient.GenerateGradient((Color)latestColor, updatedColor, 50); //ColorTools.GetIntervalColor((Color)latestColor, (Color)ColorConverter.ConvertFromString(hex.Text), 50);

                Runtime.TaskRun(() =>
                { 
                    var i = 0;
                    foreach (var newColor in intervalColors) 
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{i}. {newColor.ToString()}");
                        App.Current.Dispatcher.Invoke(() => { 
                            App.Current.Resources["DefaultButtonBackgroundColor"] = newColor;
                            App.Current.Resources["DefaultButtonBackgroundColorBrush"] = new SolidColorBrush(newColor);
                        });
                        App.Current.Resources["HoverAccentColor1"] = ColorTools.GetDarkerColor(newColor, 10);
                        App.Current.Resources["HoverAccentColor"] = new SolidColorBrush((Color)App.Current.Resources["HoverAccentColor1"]);
                        App.Current.Resources["PressedAccentColor1"] = ColorTools.GetDarkerColor(newColor, 20);
                        App.Current.Resources["PressedAccentColor"] = new SolidColorBrush((Color)App.Current.Resources["PressedAccentColor1"]);
                        App.Current.Resources["DisableAccentColor1"] = ColorTools.GetDarkerColor(newColor, 70);
                        App.Current.Resources["DisableAccentColor"] = new SolidColorBrush((Color)App.Current.Resources["DisableAccentColor1"]);

                        App.Current.Resources["ForegroundAccentColor1"] = ColorTools.ForegroundByColor(newColor);
                        App.Current.Resources["ForegroundAccentColor"] = new SolidColorBrush((Color)App.Current.Resources["ForegroundAccentColor1"]);
                        Thread.Sleep(10);
                        i++;
                    }
                    Thread.Sleep(10);
                    App.Current.Dispatcher.Invoke(() => {
                        App.Current.Resources["DefaultButtonBackgroundColor"] = updatedColor;
                        App.Current.Resources["DefaultButtonBackgroundColorBrush"] = new SolidColorBrush(updatedColor);
                    });
                });
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(App.Current.Resources["AccentColor1"]);
            }
            catch (Exception ea) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ea);
                Console.ForegroundColor = ConsoleColor.White;
            }

            // #13f287
            // #167FFC
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(StartTB.Text))
                StartTB.Text = "#6501FF";
            if (string.IsNullOrEmpty(EndTB.Text))
                EndTB.Text = "#FEFF01";


                var start = (Color)ColorConverter.ConvertFromString(StartTB.Text);
            var end = (Color)ColorConverter.ConvertFromString(EndTB.Text);

            Colors.ItemsSource = Gradient.GenerateGradient(start, end, 50);
        }
    }
}

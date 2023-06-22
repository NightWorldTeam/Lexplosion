using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lexplosion.WPF.NewInterface.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static int i = 0;

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
                Console.WriteLine(i.ToString() + " " + grid.ColumnDefinitions[i].ActualWidth.ToString());
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Runtime.TaskRun(() => { 
                while (i == 0)
                {
                    Console.WriteLine(i);
                }
            });

            var newColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#8500fa");

            ColorAnimation colorAnimation = new ColorAnimation();
            colorAnimation.From = (Color)Application.Current.Resources["ActivityColor"];
            colorAnimation.To = Colors.Red;
            colorAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));
            colorAnimation.Completed += ColorAnimation_Completed;
            ChangeColorButton.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            
        }

        private void ColorAnimation_Completed(object sender, EventArgs e)
        {
            i = 1;
            Console.WriteLine(1);
        }
    }
}

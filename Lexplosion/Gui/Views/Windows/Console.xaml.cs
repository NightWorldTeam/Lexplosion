using Lexplosion.Logic.Management;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lexplosion.Gui.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для Console.xaml
    /// </summary>
    public partial class Console : Window
    {
        private Paragraph paragraph;

        public Console()
        {
            InitializeComponent();
            MouseDown += delegate { try { DragMove(); } catch { } };

            paragraph = new Paragraph();
            LaunchGame.ProcessDataReceived += AddNewLine;
            //ConsoleOutput.Document.PageWidth = ConsoleOutput.Width;
            //ConsoleOutput.Document.Blocks.Add(AddNewLine("[10:20:23.159 ERROR]: Unknown error when was launch prepare minecraft client", "#f7a737", 0.3, "#f7a737"));
            //var paragraph = new Paragraph();
            //var run = new Run(text);

            //var solidBrush = new SolidColorBrush();
            //run.Background = solidBrush;

            //run.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom(foregroundHex);


            //run.FontSize = 14;
            //solidBrush.Opacity = 0.3;
            //solidBrush.Color = (Color)ColorConverter.ConvertFromString(backgroundHex);


            //paragraph.Inlines.Add(run);
        }

        private void AddNewLine(string text)
        {
            App.Current.Dispatcher.Invoke(() => { 
                //
                //paragraph.Inlines.Add(text + "\n");
                //ConsoleOutput.Document.Blocks.Add(paragraph);
                //if (ConsoleOutput.ScrollToVerticalOffset != ConsoleOutput.)
                //ConsoleOutput.ScrollToEnd();
            });
        }
    }
}

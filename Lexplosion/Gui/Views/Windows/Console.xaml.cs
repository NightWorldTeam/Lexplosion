using Lexplosion.Logic.Management;
using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Lexplosion.Gui.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для Console.xaml
    /// </summary>
    public partial class Console : Window
    {
        private Paragraph _paragraph;
        private bool _isLastLineError;

        public Console()
        {
            InitializeComponent();
            MouseDown += delegate { try { DragMove(); } catch { } };

            _paragraph = new Paragraph();
            LaunchGame.ProcessDataReceived += AddNewLine;

            ConsoleOutput.ScrollToVerticalOffset(Double.PositiveInfinity);

            ConsoleOutput.Document.Blocks.Add(_paragraph);
        }

        private Run GetRun(string text, string foregroundHex, string backgroundHex, double opacity = 0.3) 
        {
            return new Run(text)
            {
                Background = new SolidColorBrush()
                {
                    Opacity = opacity,
                    Color = (Color)ColorConverter.ConvertFromString(backgroundHex)
                },
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom(foregroundHex),
                FontSize = 14
            };
        }

        private void AddNewLine(string text)
        {
            if (text == null)
                return;

            App.Current.Dispatcher.Invoke(() => 
            {
                var isEnd = ConsoleOutput.VerticalOffset == Double.PositiveInfinity;

                if (text[0] == '[') 
                {
                    _isLastLineError = false;
                }

                if (text.Contains("/WARN"))
                {
                    _isLastLineError = false;
                    _paragraph.Inlines.Add(GetRun(text, "#e59f38", "#e59f38"));
                    _paragraph.Inlines.Add(new LineBreak());
                }
                else if (text.Contains("/INFO"))
                {
                    _isLastLineError = false;
                    _paragraph.Inlines.Add(GetRun(text, "#167FFC", "#167FFC", 0.1));
                    _paragraph.Inlines.Add(new LineBreak());
                }
                else if (text.Contains("/ERROR") || _isLastLineError || text.Contains("Exception: "))
                {
                    _isLastLineError = true;
                    _paragraph.Inlines.Add(GetRun(text, "#FF0000", "#c94b4b"));
                    _paragraph.Inlines.Add(new LineBreak());
                }
                else
                {
                    _paragraph.Inlines.Add(GetRun(text, "#a6a6a6", "#2d343d"));
                    _paragraph.Inlines.Add(new LineBreak());
                }

                ConsoleOutput.ScrollToEnd();
            });
        }
    }
}

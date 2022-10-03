using Lexplosion.Logic.Management;
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
        private LaunchGame _gameManager;


        #region Constructor

        public Console(LaunchGame gameManager)
        {
            InitializeComponent();
            MouseDown += delegate { try { DragMove(); } catch { } };

            _paragraph = new Paragraph();
            _gameManager = gameManager;
            gameManager.ProcessDataReceived += AddNewLine;

            ConsoleOutput.Document.Blocks.Add(_paragraph);
        }

        #endregion Constructor


        #region Private Methods

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
            if (string.IsNullOrEmpty(text))
                return;

            App.Current.Dispatcher.Invoke(() => 
            {
                //var isEnd = ConsoleOutput.VerticalOffset == Double.PositiveInfinity;

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

                //if (_isFirstLine)
                //{
                //    _isFirstLine = false;
                //    ConsoleOutputScrollViewer.ScrollToBottom();
                //}

                //if (isEnd)
                    ConsoleOutputScrollViewer.ScrollToEnd();
            });
        }

        #region Header Menu


        /// <summary>
        /// Закрывает окно консоли.
        /// </summary>
        internal void Exit(object sender, RoutedEventArgs e)
        {
            _gameManager.ProcessDataReceived -= AddNewLine;
            ConsoleOutput.Document.Blocks.Clear();
            this.Close();
        }

        /// <summary>
        /// Сворачиваем консоль.
        /// </summary>
        private void Hide(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }


        #endregion Header Menu

        #endregion Private Methods
    }
}

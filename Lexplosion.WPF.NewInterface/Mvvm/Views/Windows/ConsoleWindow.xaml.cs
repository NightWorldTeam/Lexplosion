using Lexplosion.Logic.Management;
using Lexplosion.WPF.NewInterface.Core;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Windows
{
    /// <summary>
    /// Interaction logic for ConsoleWindow.xaml
    /// </summary>
    public partial class ConsoleWindow : Window, IScalable
    {
        private LaunchGame _gameManager;
        private static ConsoleWindow _classInstance;
        
        public ObservableCollection<ConsoleLog> Logs { get; } = [];

        public double ScalingKeff { get; private set; } = 1;
        public double ScalingFactor { get; private set; } = 1;

        private StringBuilder _allStringContent = new();
        private bool _hasSelectedItems;
        private ICollection _selectedLogs;

        // TODO: Подумать, возможно код отсюда вынести в ViewModel, для большей расширяемости

        #region Constructors


        public ConsoleWindow(LaunchGame gameManager)
        {
            InitializeComponent();
            MouseDown += delegate { try { DragMove(); } catch { } };

            _gameManager = gameManager;
            LogsContainer.ItemsSource = Logs;
            Logs.CollectionChanged += Logs_CollectionChanged;
            LogsContainer.SelectionChanged += LogsContainer_SelectionChanged;
            Closed += OnClosed;
        }


        #endregion Constructors


        public static void SetWindow(LaunchGame gameManager)
        {
            if (_classInstance == null)
            {
                _classInstance = new ConsoleWindow(gameManager)
                {
                    Left = App.Current.MainWindow.Left + 322,
                    Top = App.Current.MainWindow.Top + 89
                };

                _classInstance.Show();
            }
            else
            {
                _classInstance._gameManager.ProcessDataReceived -= _classInstance.AddNewLine;
            }

            _classInstance.SetGameManager(gameManager);
        }


        #region Private Methods


        private void SetGameManager(LaunchGame gameManager)
        {
            _gameManager = gameManager;
            gameManager.ProcessDataReceived += AddNewLine;
        }

        bool _isLastLineError;

        private void AddNewLine(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            App.Current.Dispatcher.Invoke(() =>
            {
                Logs.Add(new(text));
                _allStringContent.AppendLine(text);

                if (VisualTreeHelper.GetChildrenCount(LogsContainer) > 0)
                {
                    Border border = (Border)VisualTreeHelper.GetChild(LogsContainer, 0);
                    ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                    scrollViewer.ScrollToBottom();
                }
            });
        }

        /// <summary>
        /// Закрывает окно консоли.
        /// </summary>
        internal void Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            _classInstance = null;
            _gameManager.ProcessDataReceived -= AddNewLine;
            _allStringContent.Clear();
        }

        private void LogsContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedLogs = LogsContainer.SelectedItems as ICollection;

            _hasSelectedItems = _selectedLogs.Count > 0;

            if (_hasSelectedItems)
            {
                FragmentCopyButton.Visibility = Visibility.Visible;

                if (Logs.Count == _selectedLogs.Count)
                {
                    SelectAllButton.Visibility = Visibility.Collapsed;
                    UnselectAllButton.Visibility = Visibility.Visible;
                }
            }
            else
            {
                SelectAllButton.Visibility = Visibility.Visible;
                UnselectAllButton.Visibility = Visibility.Collapsed;
                FragmentCopyButton.Visibility = Visibility.Collapsed;
            }
        }

        private void Logs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            LogsContainer.AlternationCount = Logs.Count;
        }


        #endregion


        #region Button Click


        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            LogsContainer.SelectAll();
        }

        private void UnselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            LogsContainer.UnselectAll();
        }

        private void CopyAllButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_allStringContent.ToString());
        }

        void Copy() 
        {
            if (_selectedLogs.Count == Logs.Count)
            {
                Clipboard.SetText(_allStringContent.ToString());
            }
            else
            {
                var stringBuilder = new StringBuilder();
                foreach (ConsoleLog log in _selectedLogs)
                {
                    stringBuilder.AppendLine(log.Message);
                }

                Clipboard.SetText(stringBuilder.ToString());
            }
        }

        private void FragmentCopyButton_Click(object sender, RoutedEventArgs e)
        {
            Copy();
        }

        private void LogsContainer_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) 
            {
                if (e.Key == Key.A) 
                {
                    LogsContainer.SelectAll();
                }

                if (e.Key == Key.C) 
                {
                    Copy();
                }
            }
        }


        #endregion Button Clicked


        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MaximazedWindow_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void MinimazedWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ScaleFit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Scalling();
        }

        private void ChangeWHPOrintation_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChangeWHPHorizontalOrintationAnimation();
        }

        private void ChangeWHPHorizontalOrintationAnimation()
        {
            var opacityAdditionalFuncsHideAnimation = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(0.35 / 2),
                To = 0
            };

            var opacityHideAnimation = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(0.35 / 2),
                To = 0
            };

            var opacityShowAnimation = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(0.35 / 2),
                To = 1
            };

            // перемещаем кнопки и панель в нужную сторону.
            opacityHideAnimation.Completed += (object sender, EventArgs e) =>
            {
                ChangeWHPHorizontalOrintation();
                WindowHeaderPanelButtonsGrid.BeginAnimation(OpacityProperty, opacityShowAnimation);
            };

            // скрываем 
            WindowHeaderPanelButtonsGrid.BeginAnimation(OpacityProperty, opacityHideAnimation);
        }

        private void ChangeWHPHorizontalOrintation()
        {
            if (WindowHeaderPanelButtonsGrid.HorizontalAlignment == HorizontalAlignment.Left)
            {
                WindowHeaderPanelButtons.RenderTransform = new RotateTransform(180);
                WindowHeaderPanelButtonsGrid.HorizontalAlignment = HorizontalAlignment.Right;

                //AddtionalFuncs.HorizontalAlignment = HorizontalAlignment.Left;

                Grid.SetColumn(DebugPanel, 0);
                Grid.SetColumn(WindowHeaderPanelButtons, 1);

                RuntimeApp.HeaderState = HeaderState.Right;
            }
            else
            {
                WindowHeaderPanelButtons.RenderTransform = new RotateTransform(360);
                WindowHeaderPanelButtonsGrid.HorizontalAlignment = HorizontalAlignment.Left;

                //AddtionalFuncs.HorizontalAlignment = HorizontalAlignment.Right;

                Grid.SetColumn(DebugPanel, 1);
                Grid.SetColumn(WindowHeaderPanelButtons, 0);

                RuntimeApp.HeaderState = HeaderState.Left;
            }
        }
        bool _isScalled = false;
        private void Scalling()
        {
            double factor = 0.25;
            var yScale = factor + 1;

            if (_isScalled)
            {
                factor *= -1;
                yScale = 1;
            }

            ContentContainer.LayoutTransform = new ScaleTransform(yScale, yScale);
            this.Width += Width * factor;
            this.Height += Height * factor;
            ScalingFactor = factor;
            // Bring window center screen
            var screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            var screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            Top = (screenHeight - Height) / 2;
            Left = (screenWidth - Width) / 2;

            _isScalled = !_isScalled;
        }
    }
}
﻿using Lexplosion.Logic.Management;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Services;
using Lexplosion.UI.WPF.WindowComponents.Header;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Lexplosion.UI.WPF.Mvvm.Views.Windows
{
    /// <summary>
    /// Interaction logic for ConsoleWindow.xaml
    /// </summary>
    public partial class ConsoleWindow : Window
    {
        private static ConsoleWindow _classInstance;
        private readonly AppCore _appCore;
        private LaunchGame _gameManager;
        private ScalingService _scalingService;
        private ObservableCollection<ConsoleLog> _logs = [];
        public FiltableObservableCollection Logs { get; } = [];

        private StringBuilder _allStringContent = new();
        private ICollection _selectedLogs;

        private bool _hasSelectedItems;
        private bool _isLastLineError;

        // TODO: Подумать, возможно код отсюда вынести в ViewModel, для большей расширяемости

        #region Constructors


        public ConsoleWindow(AppCore appCore, LaunchGame gameManager)
        {
            InitializeComponent();
            MouseDown += delegate { try { DragMove(); } catch { } };


            _appCore = appCore;
            _gameManager = gameManager;
            Logs.Source = _logs;
            LogsContainer.ItemsSource = Logs;
            LogsContainer.SelectionChanged += LogsContainer_SelectionChanged;

            InstanceNamePlaceholder.Text = _gameManager.GameClientName;
            GameVersionPlaceholder.Text = _gameManager.GameVersion;
            //ModloaderPlaceholder.Text = _gameManager.

            WHC.DataContext = new WindowHeaderArgs(
                "WindowsOS",
                () => CloseWindow_Click(null, null),
                () => MaximazedWindow_Click(null, null),
                () => MinimazedWindow_Click(null, null),
                false);

            Loaded += ConsoleWindow_Loaded;
        }

        private void ConsoleWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _scalingService = new ScalingService(_appCore, this, ContainerGrid);
            _scalingService.ChangeNoFactorSizeValues(Width, Height);
            _scalingService.Rescale(this, ContainerGrid);
        }




        #endregion Constructors


        public static void SetWindow(AppCore appCore, LaunchGame gameManager)
        {
            if (_classInstance == null)
            {
                _classInstance = new ConsoleWindow(appCore, gameManager)
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
            _logs.Clear();
            _gameManager = gameManager;
            gameManager.ProcessDataReceived += AddNewLine;
        }


        private void AddNewLine(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            App.Current.Dispatcher.Invoke(() =>
            {
                _logs.Add(new ConsoleLog(text));
                _allStringContent.AppendLine(text);

                if (VisualTreeHelper.GetChildrenCount(LogsContainer) > 0)
                {
                    Border border = (Border)VisualTreeHelper.GetChild(LogsContainer, 0);
                    ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                    scrollViewer.ScrollToBottom();
                }
            });
        }

        private void LogsContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedLogs = LogsContainer.SelectedItems as ICollection;

            _hasSelectedItems = _selectedLogs.Count > 0;

            if (_hasSelectedItems)
            {
                FragmentCopyButton.Visibility = Visibility.Visible;

                if (_logs.Count == _selectedLogs.Count)
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
            _classInstance = null;
            _gameManager.ProcessDataReceived -= AddNewLine;
            _allStringContent.Clear();
            Close();
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

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = (sender as TextBox);
            Logs.Filter = (i =>
            {
                return (i as ConsoleLog).Message.IndexOf(tb.Text, System.StringComparison.InvariantCultureIgnoreCase) > -1;
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var stringBuilder = new StringBuilder();
            foreach (ConsoleLog log in _selectedLogs ?? _logs)
            {
                stringBuilder.AppendLine(log.Message);
            }

            var dialog = new System.Windows.Forms.SaveFileDialog()
            {
                FileName = $"{_gameManager.GameClientName} {_gameManager.GameVersion} {DateTime.Now}".Replace(":", "_"),
                Filter = "Text Files(*.txt)|*.txt|All(*.*)|*"
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(dialog.FileName, stringBuilder.ToString());
            }
        }
    }
}
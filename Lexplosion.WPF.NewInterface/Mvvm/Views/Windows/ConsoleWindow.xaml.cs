using Lexplosion.Logic.Management;
using Lexplosion.WPF.NewInterface.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Windows
{
    /// <summary>
    /// Interaction logic for ConsoleWindow.xaml
    /// </summary>
    public partial class ConsoleWindow : Window
    {
        private LaunchGame _gameManager;
        private static ConsoleWindow _classInstance;
        
        public ObservableCollection<ConsoleLog> Logs { get; } = [];
        private StringBuilder _allStringContent = new();
        private bool _hasSelectedItems;
        private ICollection _selectedLogs;

        public ConsoleWindow(LaunchGame gameManager)
        {
            InitializeComponent();
            _gameManager = gameManager;

            LogsContainer.ItemsSource = Logs;
            Logs.CollectionChanged += Logs_CollectionChanged;
            LogsContainer.SelectionChanged += LogsContainer_SelectionChanged;
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
            });
        }

        #endregion


        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            LogsContainer.SelectAll();
        }

        private void UnselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            LogsContainer.UnselectAll();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

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
    }
}
using Lexplosion.Logic.Management;
using Lexplosion.WPF.NewInterface.Core;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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

        public ConsoleWindow(LaunchGame gameManager)
        {
            InitializeComponent();
            _gameManager = gameManager;

            //var binding = new Binding)()

            LogsContainer.ItemsSource = Logs;// (ItemsControl.ItemsSourceProperty, Logs);
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
            });
        }

        #endregion
    }
}
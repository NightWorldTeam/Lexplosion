using Lexplosion.Gui.Models;
using Lexplosion.Gui.ViewModels.ModalVMs;
using Lexplosion.Logic.FileSystem;
using System;
using System.Windows.Forms;

namespace Lexplosion.Gui.ViewModels.MainMenu.Settings
{
    public class GeneralSettingsViewModel : VMBase
    {
        private MainViewModel _mainViewModel;

        public GeneralSettingsModel GeneralSettings { get; set; }

        private bool _isDirectoryChanged = true;
        public bool IsDirectoryChanged
        {
            get => _isDirectoryChanged; set
            {
                _isDirectoryChanged = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand OpenFolderBrowser
        {
            get => new RelayCommand(obj =>
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.SelectedPath = GeneralSettings.SystemPath.Replace("/", @"\");
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        ChangedDirectory(dialog.SelectedPath);
                    }
                }
            });
        }

        public RelayCommand OpenJavaFolderBrowser
        {
            get => new RelayCommand(obj =>
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.SelectedPath = GeneralSettings.JavaPath.Replace("/", @"\");
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        GeneralSettings.JavaPath = dialog.SelectedPath;
                    }
                }
            });
        }

        public RelayCommand SetDefaultJavaPath
        {
            get => new RelayCommand(obj =>
            {
                GeneralSettings.JavaPath = "";
            });
        }

        private void ChangedDirectory(string newPath)
        {
            GeneralSettings.SystemPath = newPath;

            var dialogModal = new DialogViewModel(_mainViewModel);
            dialogModal.ShowDialog("Test", "Желаете ли вы полностью перенести директорию?", () =>
            {
                IsDirectoryChanged = false;
                Lexplosion.Runtime.TaskRun(() =>
                {
                    WithDirectory.SetNewDirectory(GeneralSettings.SystemPath);
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        MainViewModel.ShowToastMessage("Настройки изменены!", "Директория для лаунчера была успешно перенесена.", TimeSpan.FromSeconds(2));
                        IsDirectoryChanged = true;
                    });
                });
            });
        }

        public GeneralSettingsViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            GeneralSettings = new GeneralSettingsModel();
        }
    }
}

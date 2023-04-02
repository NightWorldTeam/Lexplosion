using Lexplosion.Global;
using Lexplosion.Gui.Models;
using Lexplosion.Gui.ViewModels.ModalVMs;
using Lexplosion.Logic.FileSystem;
using System;
using System.Windows.Forms;

namespace Lexplosion.Gui.ViewModels.MainMenu.Settings
{
    public class GeneralSettingsViewModel : VMBase
    {
        private readonly Action<string, string, uint, byte> _doNotification = (header, message, time, type) => { };

        // Model
        public GeneralSettingsModel GeneralSettings { get; set; }


        #region Properties


        private bool _isDirectoryChanged = true;
        public bool IsDirectoryChanged
        {
            get => _isDirectoryChanged; set
            {
                _isDirectoryChanged = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Commands


        public RelayCommand OpenFolderBrowser
        {
            get => new RelayCommand(obj =>
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.SelectedPath = GeneralSettings.SystemPath.Replace('/', '\\');
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
                    dialog.SelectedPath = GeneralSettings.JavaPath.Replace('/', '\\');
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


        #endregion Commands


        #region Constructors


        public GeneralSettingsViewModel(Action<string, string, uint, byte> doNotification)
        {
            _doNotification = doNotification ?? _doNotification;
            GeneralSettings = new GeneralSettingsModel();
        }


        #endregion Constructors


        #region Private Methods


        private void ChangedDirectory(string newPath)
        {
            newPath = newPath + "/" + LaunсherSettings.GAME_FOLDER_NAME;
            GeneralSettings.SystemPath = newPath;

            var dialogModal = new DialogViewModel();
            dialogModal.ShowDialog("Перенос директории", "Желаете ли вы полностью перенести директорию?", () =>
            {
                IsDirectoryChanged = false;
                Lexplosion.Runtime.TaskRun(() =>
                {
                    WithDirectory.SetNewDirectory(GeneralSettings.SystemPath);
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _doNotification("Настройки изменены!", "Директория для лаунчера была успешно перенесена.", 2, 0);
                        IsDirectoryChanged = true;
                    });
                });
            });
        }


        #endregion Private Methods
    }
}

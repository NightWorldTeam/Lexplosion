using Lexplosion.Common.Models;
using Lexplosion.Common.ViewModels.ModalVMs;
using Lexplosion.Controls;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Tools;
using System.Windows.Forms;

namespace Lexplosion.Common.ViewModels.MainMenu.Settings
{
    public class GeneralSettingsViewModel : VMBase
    {
        private readonly DoNotificationCallback _doNotification = (header, message, time, type) => { };


        #region Properties


        public GeneralSettingsModel Model { get; set; }

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
                    dialog.SelectedPath = Model.SystemPath.Replace('/', '\\');
                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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
                    dialog.SelectedPath = Model.JavaPath.Replace('/', '\\');
                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Model.JavaPath = dialog.SelectedPath;
                    }
                }
            });
        }

        public RelayCommand OpenJava17FolderBrowser
        {
            get => new RelayCommand(obj =>
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.SelectedPath = Model.Java17Path.Replace('/', '\\');
                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Model.Java17Path = dialog.SelectedPath;
                    }
                }
            });
        }

        private RelayCommand _resetJavaPath;
        public RelayCommand ResetJavaPath
        {
            get => _resetJavaPath ?? (_resetJavaPath = new RelayCommand(obj =>
            {
                Model.ResetJavaPath();
            }));
        }

        private RelayCommand _resetJava17Path;
        public RelayCommand ResetJava17Path
        {
            get => _resetJava17Path ?? (_resetJava17Path = new RelayCommand(obj =>
            {
                Model.ResetJava17Path();
            }));
        }


        #endregion Commands


        #region Constructors


        public GeneralSettingsViewModel(DoNotificationCallback doNotification)
        {
            _doNotification = doNotification ?? _doNotification;
            Model = new GeneralSettingsModel(doNotification);
        }


        #endregion Constructors


        #region Private Methods


        private void ChangedDirectory(string newPath)
        {
            newPath = newPath + "/" + LaunсherSettings.GAME_FOLDER_NAME;
            Model.SystemPath = newPath;

            var dialogModal = new DialogViewModel();
            dialogModal.ShowDialog(ResourceGetter.GetString("directoryTransfer"), ResourceGetter.GetString("doYouWantToFullDirectoryTransfer"), () =>
            {
                IsDirectoryChanged = false;
                Lexplosion.Runtime.TaskRun(() =>
                {
                    WithDirectory.SetNewDirectory(Model.SystemPath);
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _doNotification(ResourceGetter.GetString("settingsChanged"), ResourceGetter.GetString("directoryWasTransfered"), 2, 0);
                        IsDirectoryChanged = true;
                    });
                });
            });
        }


        #endregion Private Methods
    }
}

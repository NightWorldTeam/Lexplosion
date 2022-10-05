using Lexplosion.Gui.Models;
using Lexplosion.Gui.ViewModels.ModalVMs;
using Lexplosion.Logic.FileSystem;
using System;
using System.Windows.Forms;

namespace Lexplosion.Gui.ViewModels.MainMenu
{
    public class GeneralSettingsViewModel : VMBase
    {
        private MainViewModel _mainViewModel;

        public GeneralSettingsModel GeneralSettings { get; set; }
        public RelayCommand OpenFolderBrowser
        {
            get => new RelayCommand(obj =>
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.SelectedPath = GeneralSettings.SystemPath.Replace("/", @"\");
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        var dialogModal = new DialogViewModel(_mainViewModel);
                        dialogModal.ShowDialog("Желаете ли вы полностью перенести директорию?", () => {
                            GeneralSettings.SystemPath = dialog.SelectedPath;
                            WithDirectory.SetNewDirectory(GeneralSettings.SystemPath);
                            MainViewModel.ShowToastMessage("Настройки изменены!", "Директория для лаунчера была успешно перенесена.", TimeSpan.FromSeconds(2));
                        });
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

        public GeneralSettingsViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            GeneralSettings = new GeneralSettingsModel();
        }
    }
}

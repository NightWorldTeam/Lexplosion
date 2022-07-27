using Lexplosion.Gui.Models;
using System.Windows.Forms;

namespace Lexplosion.Gui.ViewModels.MainMenu
{
    public class GeneralSettingsViewModel : VMBase
    {
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
                        GeneralSettings.SystemPath = dialog.SelectedPath;
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

        public GeneralSettingsViewModel()
        {
            GeneralSettings = new GeneralSettingsModel();
        }
    }
}

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
                    dialog.SelectedPath = GeneralSettings.SystemPath;
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        GeneralSettings.SystemPath = dialog.SelectedPath;
                    }
                }
            });
        }

        public GeneralSettingsViewModel()
        {
            GeneralSettings = new GeneralSettingsModel();
        }
    }
}

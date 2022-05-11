using Lexplosion.Gui.Models.ShowCaseMenu;
using System.Windows.Forms;

namespace Lexplosion.Gui.ViewModels.ShowCaseMenu
{
    public class InstanceSettingsViewModel : VMBase
    {
        public InstanceSettingsModel InstanceSettings { get; set; }
        public RelayCommand OpenFolderBrowser
        {
            get => new RelayCommand(obj =>
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.SelectedPath = InstanceSettings.JavaPath;
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        InstanceSettings.JavaPath = dialog.SelectedPath;
                    }
                }
            });
        }

        public InstanceSettingsViewModel(string instanceId)
        {
            InstanceSettings = new InstanceSettingsModel(instanceId);
        }
    }
}

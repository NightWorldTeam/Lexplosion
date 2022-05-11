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
                    //dialog.SelectedPath = InstanceSettings.SystemPath;
                    //if (dialog.ShowDialog() == DialogResult.OK)
                    //{
                    //    InstanceSettings.SystemPath = dialog.SelectedPath;
                    //}
                }
            });
        }

        public InstanceSettingsViewModel(string instanceId)
        {
            InstanceSettings = new InstanceSettingsModel(instanceId);
        }
    }
}

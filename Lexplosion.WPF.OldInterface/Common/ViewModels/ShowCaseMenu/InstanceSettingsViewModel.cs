using Lexplosion.Common.Models.ShowCaseMenu;
using Lexplosion.Logic.Management.Instances;
using System.Windows.Forms;

namespace Lexplosion.Common.ViewModels.ShowCaseMenu
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

        public RelayCommand OpenJavaFolderBrowser
        {
            get => new RelayCommand(obj =>
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.SelectedPath = InstanceSettings.JavaPath.Replace("/", @"\");
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        InstanceSettings.JavaPath = dialog.SelectedPath;
                    }
                }
            });
        }

        public RelayCommand SetDefaultJavaPath
        {
            get => new RelayCommand(obj =>
            {
                InstanceSettings.JavaPath = "";
            });
        }

        public InstanceSettingsViewModel(InstanceClient instanceClient)
        {
            InstanceSettings = new InstanceSettingsModel(instanceClient);
        }
    }
}

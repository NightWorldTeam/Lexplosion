using Lexplosion.Logic.Management.Instances;
using System.Collections.ObjectModel;

namespace Lexplosion.Common.Models.Objects
{
    public sealed class DownloadAddonFile
    {
        public InstanceAddon InstanceAddon { get; }

        private RelayCommand _cancelDownloadCommand;
        public RelayCommand CancelDownloadCommand
        {
            get => _cancelDownloadCommand ?? (_cancelDownloadCommand = new RelayCommand(obj =>
            {
                InstanceAddon.CancellDownload();
            }));
        }


        public DownloadAddonFile(InstanceAddon instanceAddon)
        {
            InstanceAddon = instanceAddon;
        }

        public bool IsRightAddon(InstanceAddon instanceAddon)
        {
            return InstanceAddon == instanceAddon;
        }

        public static void Remove(ObservableCollection<DownloadAddonFile> addonFiles, InstanceAddon instanceAddon)
        {
            foreach (var addonFile in addonFiles)
            {
                if (addonFile.InstanceAddon == instanceAddon)
                {
                    addonFiles.Remove(addonFile);
                    break;
                }
            }
        }
    }
}

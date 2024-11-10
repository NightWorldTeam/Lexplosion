using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Commands;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public class DownloableAddonFile
    {
        public InstanceAddon Addon { get; set; }


        private RelayCommand _cancelCommand;
        public ICommand CancelCommand
        {
            get => RelayCommand.GetCommand(ref _cancelCommand, () =>
            {
                Addon.CancellDownload();
            });
        }


        public DownloableAddonFile(InstanceAddon addon)
        {
            Addon = addon;
        }

        public override int GetHashCode()
        {
            return Addon.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is DownloableAddonFile file) 
            {
                return file.Addon.Equals(Addon);
            }

            return false;
        }
    }
}

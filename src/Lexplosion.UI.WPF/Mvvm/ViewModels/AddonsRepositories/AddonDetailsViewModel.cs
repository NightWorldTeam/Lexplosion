using Lexplosion.Logic.Management.Addons;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.AddonsRepositories;
using System;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.AddonsRepositories
{
    public sealed class AddonDetailsViewModel : ViewModelBase
    {
        public AddonDetailsModel Model { get; }

        public ICommand InstallAddonCommand { get; }
        public ICommand UninstallAddonCommand { get; }
        public ICommand OpenExternalResourceCommand { get; }
        public ICommand CloseAddonDetailsCommand { get; }

        private RelayCommand _installSelectedAddonVersionCommand;
        public ICommand InstallSelectedAddonVersionCommand
        {
            get => RelayCommand.GetCommand(ref _installSelectedAddonVersionCommand, Model.InstallSelectedVersion);
        }

        private RelayCommand _toggleInformationCommand;
        public ICommand ToggleInformationCommand
        {
            get => RelayCommand.GetCommand(ref _toggleInformationCommand, Model.ToggleInformation);
        }

        private RelayCommand _closeInformationCommand;
        public ICommand CloseInformationCommand
        {
            get => RelayCommand.GetCommand(ref _closeInformationCommand, Model.CloseInformation);
        }

        private RelayCommand _openImageCommand;
        public ICommand OpenImageCommand
        {
            get => RelayCommand.GetCommand<object>(ref _openImageCommand, Model.OpenImage);
        }

        public AddonDetailsViewModel(InstanceAddon addon, Action<object> installVersion, ICommand installAddonCommand, ICommand uninstallAddonCommand, ICommand openExternalResourceCommand, ICommand closeAddonDetailsCommand, Gallery gallery)
        {
            Model = new AddonDetailsModel(addon, installVersion, gallery);
            InstallAddonCommand = installAddonCommand;
            UninstallAddonCommand = uninstallAddonCommand;
            OpenExternalResourceCommand = openExternalResourceCommand;
            CloseAddonDetailsCommand = closeAddonDetailsCommand;
        }
    }
}

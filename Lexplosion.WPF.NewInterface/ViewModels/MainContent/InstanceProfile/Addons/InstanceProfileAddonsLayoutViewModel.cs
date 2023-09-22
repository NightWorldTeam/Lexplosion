using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Models.InstanceModel;
using Lexplosion.WPF.NewInterface.Stores;
using Lexplosion.WPF.NewInterface.ViewModels.Modal;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.InstanceProfile
{
    public sealed class InstanceAddonsContainerModel 
    {
        public AddonType Type { get; } 

        private ObservableCollection<InstanceAddon> _addonsList = new ObservableCollection<InstanceAddon>();
        public IEnumerable<InstanceAddon> AddonsList { get => _addonsList; }

        public InstanceAddonsContainerModel(AddonType type)
        {
            Type = type;
        }


        #region Public Methods


        public void SetAddons(IEnumerable<InstanceAddon> addons) 
        {
            App.Current.Dispatcher?.Invoke(() => { 
                foreach (var addon in addons)
                {
                    _addonsList.Add(addon);
                }
            });
        }


        public void OpenFolderAddon(object instanceAddon)
        {
            try
            {
                if (instanceAddon is InstanceAddon)
                    System.Diagnostics.Process.Start(((InstanceAddon)instanceAddon).WebsiteUrl);   
            }
            catch
            {

            }
        }


        public void UpdateAddon(object instanceAddon) 
        {
            if (instanceAddon is InstanceAddon)
                ((InstanceAddon)instanceAddon).Update();
        }

        public void UninstallAddon(object instanceAddon)
        {
            if (instanceAddon is InstanceAddon)
                ((InstanceAddon)instanceAddon).Delete();
        }


        #endregion Public Methods
    }

    public sealed class InstanceAddonsContainerViewModel : ViewModelBase
    {
        public InstanceAddonsContainerModel Model { get; private set; }


        #region Commands


        private RelayCommand _openFolderCommand;
        public ICommand OpenFolderCommand
        {
            get => RelayCommand.GetCommand(ref _openFolderCommand, Model.UninstallAddon);
        }

        private RelayCommand _updateCommand;
        public ICommand UpdateCommand 
        {
            get => RelayCommand.GetCommand(ref _updateCommand, Model.UpdateAddon);
        }

        private RelayCommand _uninstallCommand;
        public ICommand UninstallCommand
        {
            get => RelayCommand.GetCommand(ref _uninstallCommand, (obj) => 
            {
                var dialogViewModel = new DialogBoxViewModel("delete", "delete",
                (obj) =>
                {
                    Model.UninstallAddon(obj);
                }, (obj) => { ModalNavigationStore.Instance.Close(); });
                ModalNavigationStore.Instance.Open(dialogViewModel);
            });
        }


        #endregion Commands


        public InstanceAddonsContainerViewModel(AddonType addonType, InstanceModelBase instanceModelBase)
        {
            Model = new InstanceAddonsContainerModel(addonType);

            Runtime.TaskRun(() => {
                switch (addonType) 
                {
                    case AddonType.Mods:
                        Model.SetAddons(InstanceAddon.GetInstalledMods(instanceModelBase.InstanceData));
                        break;
                    case AddonType.Resourcepacks:
                        Model.SetAddons(InstanceAddon.GetInstalledResourcepacks(instanceModelBase.InstanceData));
                        break;
                    case AddonType.Maps:
                        Model.SetAddons(InstanceAddon.GetInstalledWorlds(instanceModelBase.InstanceData));
                        break;
                    case AddonType.Shaders:
                        Model.SetAddons(InstanceAddon.GetInstalledMods(instanceModelBase.InstanceData));
                        break;
                    default:
                        Model.SetAddons(InstanceAddon.GetInstalledMods(instanceModelBase.InstanceData));
                        break;
                }
            });
        }
    }

    public sealed class InstanceProfileAddonsLayoutViewModel : ContentLayoutViewModelBase
    {
        private readonly ViewModelBase _modsViewModel;
        private readonly ViewModelBase _resourcepacksViewModel;
        private readonly ViewModelBase _mapsViewModel;
        private readonly ViewModelBase _shadersViewModel;

        public InstanceProfileAddonsLayoutViewModel(InstanceModelBase instanceModelBase) : base()
        {
            HeaderKey = "Addons";
            _modsViewModel = new InstanceAddonsContainerViewModel(AddonType.Mods, instanceModelBase);
            _resourcepacksViewModel = new InstanceAddonsContainerViewModel(AddonType.Resourcepacks, instanceModelBase);
            _mapsViewModel = new InstanceAddonsContainerViewModel(AddonType.Maps, instanceModelBase);
            _shadersViewModel = new InstanceAddonsContainerViewModel(AddonType.Shaders, instanceModelBase);
            InitAddonsTabMenu(instanceModelBase);
        }

        private void InitAddonsTabMenu(InstanceModelBase instanceModelBase)
        {
            _tabs.Add(new TabItemModel { TextKey = "Mods", Content = _modsViewModel, IsSelected = true });
            _tabs.Add(new TabItemModel { TextKey = "Resourcepacks", Content = _resourcepacksViewModel });
            _tabs.Add(new TabItemModel { TextKey = "Maps", Content = _mapsViewModel });
            _tabs.Add(new TabItemModel { TextKey = "Shaders", Content = _shadersViewModel });
        }
    }
}

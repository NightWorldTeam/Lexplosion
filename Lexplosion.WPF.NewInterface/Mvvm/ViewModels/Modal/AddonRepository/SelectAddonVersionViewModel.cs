using Lexplosion.Logic.Management.Addons;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{
    public sealed class SelectAddonVersionModel : ViewModelBase
    {
        private readonly InstanceAddon _instanceAddon;

        public string AddonName { get => _instanceAddon.Name; }

        public ObservableCollection<object> AddonVersions { get; } = [];

        public bool CanInstall { get => (!IsInstallLatestVersion && SelectedAddonVersion != null) || IsInstallLatestVersion; }

        public bool HasVersions { get; set; }

        private object _selectedAddonVersion;
        public object SelectedAddonVersion
        {
            get => _selectedAddonVersion; set
            {
                _selectedAddonVersion = value;
                OnPropertyChanged(nameof(_selectedAddonVersion));
                OnPropertyChanged(nameof(CanInstall));
            }
        }

        private bool _isInstallLatestVersion = true;
        public bool IsInstallLatestVersion
        {
            get => _isInstallLatestVersion; set
            {
                _isInstallLatestVersion = value;

                if (value)
                {
                    SelectedAddonVersion = null;
                }
                else
                {
                    SelectedAddonVersion = AddonVersions[0];
                }

                OnPropertyChanged(nameof(CanInstall));
            }
        }

        public SelectAddonVersionModel(InstanceAddon instanceAddon, IEnumerable<Modloader> modloaders)
        {
            _instanceAddon = instanceAddon;
            AddonVersions = new(instanceAddon.GetAllVersion(modloaders).Values.ToArray());
            OnPropertyChanged(nameof(AddonVersions));

            HasVersions = AddonVersions.Count > 0;
            OnPropertyChanged(nameof(HasVersions));
        }

        public void InstallAddon(Action<InstanceAddon, object> install)
        {
            install?.Invoke(_instanceAddon, SelectedAddonVersion);
        }
    }

    public sealed class SelectAddonVersionViewModel : ActionModalViewModelBase
    {
        public SelectAddonVersionModel Model { get; }

        public SelectAddonVersionViewModel(InstanceAddon instanceAddon, Action<InstanceAddon, object> install, IEnumerable<Modloader> modloaders)
        {
            Model = new(instanceAddon, modloaders);

            ActionCommandExecutedEvent += (obj) => Model.InstallAddon(install);
        }
    }
}

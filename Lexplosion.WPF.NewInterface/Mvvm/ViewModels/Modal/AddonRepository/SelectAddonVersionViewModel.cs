using Lexplosion.Logic.Management.Addons;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{
    public sealed class SelectAddonVersionModel : ViewModelBase
    {
        private readonly InstanceAddon _instanceAddon;
        private readonly IEnumerable<Modloader> _modloaders;

        /// <summary>
        /// Список версийы
        /// </summary>
        public ObservableCollection<object> AddonVersions { get; private set; } = null;
        /// <summary>
        /// Название аддона
        /// </summary>
        public string AddonName { get => _instanceAddon.Name; }
        /// <summary>
        /// Наличие версий аддона
        /// </summary>
        public bool HasVersions { get; set; } = true;
        /// <summary>
        /// Можно ли установить
        /// </summary>
        public bool CanInstall { get => (!IsInstallLatestVersion && SelectedAddonVersion != null) || IsInstallLatestVersion; }

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

        private bool _isVersionLoading;
        public bool IsVersionLoading
        {
            get => _isVersionLoading; set
            {
                _isVersionLoading = value;
                OnPropertyChanged();
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
                    if (AddonVersions == null)
                    {
                        IsVersionLoading = true;
                        Runtime.TaskRun(() =>
                        {
                            AddonVersions = new(_instanceAddon.GetAllVersion(_modloaders).Values.ToArray());
                            OnPropertyChanged(nameof(AddonVersions));

                            HasVersions = AddonVersions.Count > 0;
                            OnPropertyChanged(nameof(HasVersions));

                            IsVersionLoading = false;

                            if (HasVersions) 
                            {
                                SelectedAddonVersion = AddonVersions[0];
                            }
                            OnPropertyChanged();
                        });
                    }
                    else
                    {
                        SelectedAddonVersion = AddonVersions[0];
                        OnPropertyChanged();
                    }
                }

                OnPropertyChanged(nameof(CanInstall));
            }
        }

        public SelectAddonVersionModel(InstanceAddon instanceAddon, IEnumerable<Modloader> modloaders)
        {
            _instanceAddon = instanceAddon;
            _modloaders = modloaders;
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

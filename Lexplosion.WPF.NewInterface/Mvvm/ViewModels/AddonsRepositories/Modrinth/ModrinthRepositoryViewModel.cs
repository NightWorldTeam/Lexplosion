using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Mvvm.Models.AddonsRepositories;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.AddonsRepositories
{
    public class ModrinthAddon
    {
        private readonly InstanceAddon _instanceAddon;

        private ObservableCollection<FrameworkElementModel> _buttons = new ObservableCollection<FrameworkElementModel>();
        public IEnumerable<FrameworkElementModel> Buttons { get => _buttons; }


        public ModrinthAddon(InstanceAddon instanceAddon)
        {
            _instanceAddon = instanceAddon;
        }

        public void LoadButtons()
        {
            if (_instanceAddon.IsUrlExist)
            {
                _buttons.Add(new FrameworkElementModel("VisitModrinth", () =>
                {
                    try 
                    { 
                        Process.Start(_instanceAddon.WebsiteUrl); 
                    }
                    catch
                    { // todo: прибраться и уведомления выводить
                    }
                }, "Modrinth"));
            }

            if (_instanceAddon.UpdateAvailable)
            {
                _buttons.Add(new FrameworkElementModel("Update", () => { _instanceAddon.Update(); }, "Update"));
            }

            if (_instanceAddon.IsInstalled)
            {
                _buttons.Add(new FrameworkElementModel("Delete", _instanceAddon.Delete, "Delete"));
            }
        }
    }

    public sealed class ModrinthRepositoryViewModel : ViewModelBase
    {
        private readonly NavigateCommand<ViewModelBase> _backToInstanceProfile;

        public ModrinthRepositoryModel Model { get; }


        #region Commands


        private RelayCommand _clearFiltersCommand;
        public ICommand ClearFiltersCommand
        {
            get => _clearFiltersCommand ?? (_clearFiltersCommand = new RelayCommand(obj =>
            {
                Model.ClearFilters();
            }));
        }

        private RelayCommand _searchCommand;
        public RelayCommand SearchCommand
        {
            get => _searchCommand ?? (_searchCommand = new RelayCommand(obj =>
            {
                Model.SearchFilter = ((string)obj);
            }));
        }

        private RelayCommand _backToInstanceProfileCommand;
        public ICommand BackToInstanceProfileCommand
        {
            get => RelayCommand.GetCommand(ref _backToInstanceProfileCommand, (obj) => { _backToInstanceProfile.Execute(obj); });
        }

        private RelayCommand _searchBoxCommand;
        public ICommand SearchBoxCommand
        {
            get => RelayCommand.GetCommand(ref _searchBoxCommand, (obj) => { Model.SearchFilter = (string)obj; });
        }

        private RelayCommand _installAddonCommand;
        public ICommand InstallAddonCommand
        {
            get => RelayCommand.GetCommand(ref _installAddonCommand, (obj) => { Model.InstallAddon((InstanceAddon)obj); });
        }

        private RelayCommand _uninstallAddonCommand;
        public ICommand UninstallAddonCommand 
        {
            get => RelayCommand.GetCommand(ref _uninstallAddonCommand, () => { });
        }


        #endregion Commands


        #region Constructors


        public ModrinthRepositoryViewModel(NavigateCommand<ViewModelBase> backCommand, AddonType addonType, InstanceModelBase instanceModelBase)
        {
            _backToInstanceProfile = backCommand;
            Model = new ModrinthRepositoryModel(instanceModelBase, addonType);
        }


        #endregion Constructors
    }
}

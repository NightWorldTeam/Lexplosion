using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Mvvm.Models.AddonsRepositories;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Lexplosion.WPF.NewInterface.Stores;
using System.Windows.Input;
using Lexplosion.Logic.Management.Addons;

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
        private readonly INavigationStore _navigationStore;


        public ModrinthRepositoryModel Model { get; private set; }

        public bool IsLoading { get; private set; }


        #region Commands


        public ICommand ToCurseforgeCommand { get; private set; }
        public ICommand BackToInstanceProfileCommand { get; }

        // paginator
        private RelayCommand _nextPageCommand;
        public ICommand NextPageCommand
        {
            get => RelayCommand.GetCommand(ref _nextPageCommand, () => { });
        }

        private RelayCommand _prevPageCommand;
        public ICommand PrevPageCommand
        {
            get => RelayCommand.GetCommand(ref _prevPageCommand, () => { });
        }

        private RelayCommand _ToPageCommand;
        public ICommand ToPageCommand
        {
            get => RelayCommand.GetCommand(ref _nextPageCommand, () => { });
        }

        // filters
        private RelayCommand _clearFiltersCommand;
        public ICommand ClearFiltersCommand
        {
            get => RelayCommand.GetCommand(ref _clearFiltersCommand, Model.ClearFilters);
        }

        private RelayCommand _searchCommand;
        public ICommand SearchCommand
        {
            get => RelayCommand.GetCommand(ref _searchCommand, (obj) => { Model.SearchFilter = obj.ToString(); });
        }

        private RelayCommand _searchBoxCommand;
        public ICommand SearchBoxCommand
        {
            get => RelayCommand.GetCommand(ref _searchBoxCommand, (obj) => { Model.SearchFilter = (string)obj; });
        }

        // instance manipulate
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

        public ICommand ApplySelectedCategoriesCommand { get; private set; }

        #endregion Commands


        #region Constructors


        public ModrinthRepositoryViewModel(BaseInstanceData instanceData, AddonType addonType, ICommand backCommand, ICommand toCurseforge)
        {
            IsLoading = true;
            
            BackToInstanceProfileCommand = backCommand;
            ToCurseforgeCommand = toCurseforge;

            Model = new ModrinthRepositoryModel(instanceData, addonType, true);

            IsLoading = false;
        }
        
        public ModrinthRepositoryViewModel(InstanceModelBase instanceModelBase, AddonType addonType, ICommand backCommand, INavigationStore navigationStore)
        {
            IsLoading = true;

            BackToInstanceProfileCommand = backCommand;
            _navigationStore = navigationStore;

            Runtime.TaskRun(() => 
            {
                var instanceData = instanceModelBase.InstanceData;
                App.Current.Dispatcher.Invoke(() => 
                {
                    var toModrinthNavCommand = new NavigateCommand<ViewModelBase>(_navigationStore, () => this);
                    var curseforgeRepository = new CurseforgeRepositoryViewModel(instanceData, addonType, backCommand, toModrinthNavCommand);

                    ToCurseforgeCommand = new NavigateCommand<ViewModelBase>(_navigationStore, () => curseforgeRepository);
                    OnPropertyChanged(nameof(ToCurseforgeCommand));

                    Model = new ModrinthRepositoryModel(instanceData, addonType, true);
                    ApplySelectedCategoriesCommand = new RelayCommand((obj) => Model.ApplyCategories());

                    OnPropertyChanged(nameof(Model));
                    OnPropertyChanged(nameof(ApplySelectedCategoriesCommand));
                });
            });

            IsLoading = false;
        }


        #endregion Constructors
    }
}

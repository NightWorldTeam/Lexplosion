using Lexplosion.Logic.Management.Addons;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.AddonsRepositories;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile;
using Lexplosion.WPF.NewInterface.Stores;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.AddonsRepositories
{
    public sealed class LexplosionAddonsRepositoryViewModel : ViewModelBase
    {
        private readonly INavigationStore _navigationStore;


        public LexplosionAddonsRepositoryModel Model { get; private set; }

        public ViewModelBase _modsViewModel { get; set; }

        public bool IsLoading { get; private set; }


        private int _selectedAddonsRepositoryIndex;
        public int SelectedAddonsRepositoryIndex
        {
            get => _selectedAddonsRepositoryIndex; set
            {
                _selectedAddonsRepositoryIndex = value;
                // сохраняем SearchFilter
                if (Model != null)
                    _repositoriesList[value].SearchFilter = Model.SearchFilter;
                Model = _repositoriesList[value];
                OnPropertyChanged(nameof(Model));
                OnPropertyChanged();
            }
        }


        private RelayCommand _launchInstance;
        public ICommand LaunchInstanceCommand
        {
            get; private set;
        }

        private RelayCommand _stopProcessCommand;
        public ICommand StopProcessCommand 
        {
            get; private set;
        }


        public IEnumerable<ProjectSource> ProjectSources { get; }


        #region Commands


        public ICommand BackToInstanceProfileCommand { get; }

        // paginator
        private RelayCommand _nextPageCommand;
        public ICommand NextPageCommand { get; private set; }
        
        private RelayCommand _prevPageCommand;
        public ICommand PrevPageCommand { get; private set; }

        private RelayCommand _toPageCommand;
        public ICommand ToPageCommand
        {
            get => RelayCommand.GetCommand(ref _toPageCommand, () => { });
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
            get => RelayCommand.GetCommand<InstanceAddon>(ref _installAddonCommand, Model.InstallAddon);
        }

        private RelayCommand _uninstallAddonCommand;
        public ICommand UninstallAddonCommand
        {
            get => RelayCommand.GetCommand<InstanceAddon>(ref _uninstallAddonCommand, Model.RemoveAddon);
        }

        private RelayCommand _applySelectedCategoriesCommand;
        public ICommand ApplySelectedCategoriesCommand { get; private set; }

        private RelayCommand _selectCategoryCommand;
        public ICommand SelectCategoryCommand
        {
            get => RelayCommand.GetCommand<IProjectCategory>(ref _selectCategoryCommand, Model.SelectCategory);
        }

        private RelayCommand _enableAddonCommand;
        public ICommand EnableAddonCommand
        {
            get => RelayCommand.GetCommand<InstanceAddon>(ref _enableAddonCommand, Model.EnableAddon);
        }

        private RelayCommand _disableAddonCommand;
        public ICommand DisableAddonCommand
        {
            get => RelayCommand.GetCommand<InstanceAddon>(ref _disableAddonCommand, Model.DisableAddon);
        }
        
        private RelayCommand _openExternalResourceCommand;
        public ICommand OpenExternalResourceCommand
        {
            get => RelayCommand.GetCommand(ref _openExternalResourceCommand, Model.OpenWebsite);
        }



        private List<LexplosionAddonsRepositoryModel> _repositoriesList = new();


        #endregion Commands


        #region Constructors


        public LexplosionAddonsRepositoryViewModel(InstanceModelBase instanceModelBase, AddonType addonType, ICommand backCommand, INavigationStore navigationStore)
        {
            IsLoading = true;

            BackToInstanceProfileCommand = new RelayCommand((obj) =>
            {
                AddonsManager.GetManager(instanceModelBase.InstanceData).ClearAddonsListCache();
                backCommand.Execute(null);
            });
            _navigationStore = navigationStore;

            ProjectSources = [ProjectSource.Modrinth, ProjectSource.Curseforge];

            Runtime.TaskRun(() =>
            {
                var instanceData = instanceModelBase.InstanceData;

                App.Current.Dispatcher.Invoke(() =>
                {
                    if (addonType != AddonType.Maps)
                    {
                        _repositoriesList.Add(new LexplosionAddonsRepositoryModel(ProjectSource.Modrinth, instanceData, addonType, instanceModelBase));
                    }
                    _repositoriesList.Add(new LexplosionAddonsRepositoryModel(ProjectSource.Curseforge, instanceData, addonType, instanceModelBase));

                    SelectedAddonsRepositoryIndex = 0;
                    InitCommands();

                    OnPropertyChanged(nameof(ApplySelectedCategoriesCommand));
                });
            });


            _modsViewModel = new InstanceAddonsContainerViewModel(navigationStore, AddonType.Mods, instanceModelBase);


            IsLoading = false;
        }


        #endregion Constructors


        #region Private Methods


        private void InitCommands() 
        {
            LaunchInstanceCommand = RelayCommand.GetCommand(ref _launchInstance, Model.LaunchInstance);
            StopProcessCommand = RelayCommand.GetCommand(ref _launchInstance, Model.StopInstanceProcess);
            ApplySelectedCategoriesCommand = RelayCommand.GetCommand(ref _applySelectedCategoriesCommand, Model.ApplyCategories);
            NextPageCommand = RelayCommand.GetCommand<uint>(ref _nextPageCommand, Model.Paginate);
            PrevPageCommand = RelayCommand.GetCommand<uint>(ref _prevPageCommand, Model.Paginate);
        }


        #endregion Private Methods
    }
}

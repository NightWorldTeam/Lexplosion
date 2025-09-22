using Lexplosion.Logic.Management.Addons;
using Lexplosion.Logic.Objects;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.AddonsRepositories;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.InstanceProfile;
using Lexplosion.UI.WPF.Stores;
using System.Collections.Generic;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.AddonsRepositories
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

                if (Model != null)
                {
                    // сохраняем SearchFilter
                    _repositoriesList[value].SearchFilter = Model.SearchFilter;
                }

                Model = _repositoriesList[value];
                UpdateCommands();
                OnPropertyChanged(nameof(Model));
                OnPropertyChanged();
            }
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
            get => RelayCommand.GetCommand<InstanceAddon>(ref _installAddonCommand, Model.InstallAddonCurrentVersion);
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
            get => RelayCommand.GetCommand<InstanceAddon>(ref _openExternalResourceCommand, Model.OpenWebsite);
        }


        private List<LexplosionAddonsRepositoryModel> _repositoriesList = new();


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


        #endregion Commands


        #region Constructors


        public LexplosionAddonsRepositoryViewModel(AppCore appCore, InstanceModelBase instanceModelBase, AddonType addonType, ICommand backCommand, INavigationStore navigationStore)
        {
            IsLoading = true;

            BackToInstanceProfileCommand = new RelayCommand((obj) =>
            {
                AddonsManager.GetManager(instanceModelBase.BaseData, Runtime.ServicesContainer).ClearAddonsListCache();
                backCommand.Execute(null);
            });
            _navigationStore = navigationStore;

            ProjectSources = [ProjectSource.Modrinth, ProjectSource.Curseforge];

            Runtime.TaskRun(() =>
            {
                var instanceData = instanceModelBase.BaseData;

                App.Current.Dispatcher.Invoke(() =>
                {
                    if (addonType == AddonType.Maps)
                    {
                        _repositoriesList.Add(new LexplosionAddonsRepositoryModel(appCore, ProjectSource.Curseforge, instanceData, addonType, instanceModelBase, true));
                    }
                    else
                    {
                        _repositoriesList.Add(new LexplosionAddonsRepositoryModel(appCore, ProjectSource.Modrinth, instanceData, addonType, instanceModelBase, true));
                        _repositoriesList.Add(new LexplosionAddonsRepositoryModel(appCore, ProjectSource.Curseforge, instanceData, addonType, instanceModelBase));
                    }

                    SelectedAddonsRepositoryIndex = 0;
                    UpdateCommands();

                    OnPropertyChanged(nameof(ApplySelectedCategoriesCommand));
                });
            });


            _modsViewModel = new InstanceAddonsContainerViewModel(appCore, addonType, instanceModelBase);


            IsLoading = false;
        }


        #endregion Constructors


        #region Private Methods


        private void UpdateCommands()
        {
            LaunchInstanceCommand = new RelayCommand((obj) => Model.LaunchInstance());
            StopProcessCommand = new RelayCommand((obj) => Model.StopInstanceProcess());
            ApplySelectedCategoriesCommand = new RelayCommand((obj) => Model.ApplyCategories());
            NextPageCommand = new RelayCommand((obj) => Model.Paginate((uint)obj));
            PrevPageCommand = new RelayCommand((obj) => Model.Paginate((uint)obj));
        }


        private void OpenAddonModpack() 
        {
            
        }


        #endregion Private Methods
    }
}

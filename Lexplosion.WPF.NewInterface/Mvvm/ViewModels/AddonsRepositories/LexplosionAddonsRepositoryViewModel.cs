using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.AddonsRepositories;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.AddonsRepositories
{
    public sealed class LexplosionAddonsRepositoryViewModel : ViewModelBase
    {
        private readonly INavigationStore _navigationStore;


        public AddonsRepositoryModel Model { get; private set; }

        public bool IsLoading { get; private set; }


        #region Commands


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


        private List<AddonsRepositoryModel> _repositoriesList = new();


        #endregion Commands


        #region Constructors


        public LexplosionAddonsRepositoryViewModel(InstanceModelBase instanceModelBase, AddonType addonType, ICommand backCommand, INavigationStore navigationStore)
        {
            IsLoading = true;

            BackToInstanceProfileCommand = backCommand;
            _navigationStore = navigationStore;

            Runtime.TaskRun(() =>
            {
                var instanceData = instanceModelBase.InstanceData;
                App.Current.Dispatcher.Invoke(() =>
                {
                    _repositoriesList.Add(new AddonsRepositoryModel(InstanceSource.Modrinth, instanceData, addonType));
                    _repositoriesList.Add(new AddonsRepositoryModel(InstanceSource.Curseforge, instanceData, addonType));

                    Model = _repositoriesList[0];

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

using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.AddonsRepositories;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Stores;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.AddonsRepositories
{
    public sealed class CurseforgeRepositoryViewModel : ViewModelBase
    {
        public CurseforgeRepositoryModel Model { get; private set; }


        public bool IsLoading { get; }


        #region Commands


        // nav
        public ICommand BackToInstanceProfileCommand { get; }
        public ICommand ToModrinthCommand { get; private set; }


        // page control
        private RelayCommand _toCommand;
        public ICommand ToPageCommand
        {
            get => RelayCommand.GetCommand<uint>(ref _toCommand, Model.OnPageIndexChanged);
        }

        private RelayCommand _nextPageCommand;
        public ICommand NextPageCommand
        {
            get => RelayCommand.GetCommand<uint>(ref _nextPageCommand, Model.OnPageIndexChanged);
        }

        private RelayCommand _previousPageCommand;
        public ICommand PreviousPageCommand
        {
            get => RelayCommand.GetCommand<uint>(ref _previousPageCommand, Model.OnPageIndexChanged);
        }

        private RelayCommand _searchCommand;
        public ICommand SearchCommand 
        {
            get => RelayCommand.GetCommand(ref _searchCommand, Model.Search);
        }


        #endregion Commands


        #region Constructors


        public CurseforgeRepositoryViewModel(InstanceModelBase instanceModel, AddonType addonType, ICommand backCommand, INavigationStore navigationStore)
        {
            IsLoading = true;

            BackToInstanceProfileCommand = backCommand;

            Runtime.TaskRun(() =>
            {
                var instanceData = instanceModel.BaseData;
                App.Current.Dispatcher.Invoke(() =>
                {
                    var toCurseforgeNavCommand = new NavigateCommand<ViewModelBase>(navigationStore, () => this);
                    var curseforgeRepository = new ModrinthRepositoryViewModel(instanceData, addonType, backCommand, toCurseforgeNavCommand);

                    ToModrinthCommand = new NavigateCommand<ViewModelBase>(navigationStore, () => curseforgeRepository);

                    Model = new CurseforgeRepositoryModel(instanceData, addonType);
                });
            });

            IsLoading = false;
        }


        public CurseforgeRepositoryViewModel(BaseInstanceData instanceData, AddonType addonType, ICommand backCommand, ICommand toModrinth)
        {
            BackToInstanceProfileCommand = backCommand;
            ToModrinthCommand = toModrinth;

            Model = new CurseforgeRepositoryModel(instanceData, addonType);
        }


        #endregion Constructors
    }
}

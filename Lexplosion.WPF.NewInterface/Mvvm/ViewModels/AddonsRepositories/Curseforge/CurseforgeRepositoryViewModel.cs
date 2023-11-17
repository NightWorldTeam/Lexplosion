using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.AddonsRepositories;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.AddonsRepositories
{
    public sealed class CurseforgeRepositoryViewModel : ViewModelBase
    {
        public CurseforgeRepositoryModel Model { get; }


        #region Commands


        // nav
        public ICommand BackToInstanceProfileCommand { get; }
        public ICommand ToModrinthCommand { get; }


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


        #endregion Commands


        #region Constructors


        public CurseforgeRepositoryViewModel(InstanceModelBase instanceModelBase, AddonType addonType, ICommand backCommand, ICommand toModrinth)
        {
            BackToInstanceProfileCommand = backCommand;
            ToModrinthCommand = toModrinth;

            Model = new CurseforgeRepositoryModel(instanceModelBase.InstanceData);
        }


        #endregion Constructors
    }
}

using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.InstanceProfile.Settings;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile
{
    public sealed class InstanceProfileAboutViewModel : ViewModelBase
    {
        public InstanceProfileAboutModel Model { get; }


        #region Commands


        private RelayCommand _rebootChangesCommand;
        public ICommand RebootChangesCommand
        {
            get => RelayCommand.GetCommand(ref _rebootChangesCommand, (obj) => { Model.ResetChanges(); });
        }

        private RelayCommand _saveChangesCommand;
        public ICommand SaveChangesCommand
        {
            get => RelayCommand.GetCommand(ref _saveChangesCommand, (obj) => { Model.SaveData(); });
        }

        private RelayCommand _setLogoPathCommand;
        public ICommand SetLogoPathCommand
        {
            get => RelayCommand.GetCommand<object>(ref _setLogoPathCommand, (obj) => { Model.OpenFileDialog(); });
        }


        #endregion Commands


        public InstanceProfileAboutViewModel(InstanceModelBase instanceModel)
        {
            Model = new InstanceProfileAboutModel(instanceModel);
        }
    }
}

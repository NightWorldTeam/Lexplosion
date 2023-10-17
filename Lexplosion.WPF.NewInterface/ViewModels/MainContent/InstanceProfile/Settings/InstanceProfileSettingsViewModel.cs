using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Models.InstanceModel;
using Lexplosion.WPF.NewInterface.Models.MainContent.InstanceProfile;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.InstanceProfile
{
    public class InstanceProfileSettingsViewModel : ViewModelBase
    {
        public InstanceProfileSettingsModel Model { get; }


        #region Commands


        private RelayCommand _openJavaFolderBrowserCommand;
        public ICommand OpenJavaFolderBrowserCommand
        {
            get => RelayCommand.GetCommand(ref _openJavaFolderBrowserCommand, Model.OpenJavaFolderBrowser);
        }


        private RelayCommand _resetJavaPathCommand;
        public ICommand ResetJavaPathCommand
        {
            get => RelayCommand.GetCommand(ref _resetJavaPathCommand, Model.ResetJavaPath);
        }


        #endregion Commands


        public InstanceProfileSettingsViewModel(InstanceModelBase instanceModel) 
        {
            Model = new InstanceProfileSettingsModel(instanceModel);
        }
    }
}

using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.MainContent.InstanceProfile.Settings;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.InstanceProfile
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

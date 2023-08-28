using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Models.MainContent.Content;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu
{
    public sealed class GeneralSettingsViewModel : ViewModelBase
    {
        public GeneralSettingsModel Model { get; }


        #region Commands


        private RelayCommand _openGameDirectoryFolderBrowserCommand;
        public ICommand OpenGameDirectoryFolderBrowserCommand
        {
            get => _openGameDirectoryFolderBrowserCommand ?? (_openGameDirectoryFolderBrowserCommand = new RelayCommand(obj =>
            {
                Model.OpenGameDirectoryFolderBrowser();
            }));
        }

        private RelayCommand _openJavaFolderBrowserCommand;
        public ICommand OpenJavaFolderBrowserCommand
        {
            get => _openJavaFolderBrowserCommand ?? (_openJavaFolderBrowserCommand = new RelayCommand(obj =>
            {
                Model.OpenJava17FolderBrowser();
            }));
        }

        private RelayCommand _openJava17FolderBrowserCommand;
        public ICommand OpenJava17FolderBrowserCommand
        {
            get => _openJava17FolderBrowserCommand ?? (_openJava17FolderBrowserCommand = new RelayCommand(obj =>
            {
                Model.OpenJava17FolderBrowser();
            }));
        }

        private RelayCommand _resetJavaPathCommand;
        public ICommand ResetJavaPathCommand
        {
            get => _resetJavaPathCommand ?? (_resetJavaPathCommand = new RelayCommand(obj =>
            {
                Model.ResetJavaPath();
            }));
        }

        private RelayCommand _resetJava17PathCommand;
        public ICommand ResetJava17PathCommand
        {
            get => _resetJava17PathCommand ?? (_resetJava17PathCommand = new RelayCommand(obj =>
            {
                Model.ResetJava17Path();
            }));
        }


        #endregion Commands


        public GeneralSettingsViewModel()
        {
            Runtime.DebugWrite("General Settings ViewModel Init");
            Model = new GeneralSettingsModel();
        }
    }
}

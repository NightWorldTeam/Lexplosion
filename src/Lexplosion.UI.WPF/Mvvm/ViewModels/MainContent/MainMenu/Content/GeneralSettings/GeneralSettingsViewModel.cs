using Lexplosion.Core.Tools;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.MainContent.Content.GeneralSettings;
using System.IO;
using System.Windows.Input;
using static Lexplosion.Core.Tools.JavaHelper;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class GeneralSettingsViewModel : ViewModelBase
    {
        private readonly AppCore _appCore;
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


        public GeneralSettingsViewModel(AppCore appCore)
        {
            _appCore = appCore;
            Model = new GeneralSettingsModel(appCore);
            Model.Notify += OnModelNotify;
        }


        #region Private Methods


        private void OnModelNotify(object obj)
        {
          if (obj is JavaPathCheckResult javaPathCheckResult) 
            {
                JavaPathCheckResultHandler(javaPathCheckResult);
            }

            if (obj is string stringError) 
            {
                if (stringError == "EmptyOrHasInvalidPathChars") 
                {
                    _appCore.MessageService.Error("GamePathEmptyOrHasInvalidChars", true, string.Join(" ", Path.GetInvalidFileNameChars()));
                }
            }
        }

        private void JavaPathCheckResultHandler(JavaPathCheckResult javaPathCheckResult) 
        {
            switch (javaPathCheckResult)
            {
                case JavaHelper.JavaPathCheckResult.EmptyOrNull:
                    {
                        _appCore.MessageService.Info("JavaPathResets", true);
                        break;
                    }
                case JavaHelper.JavaPathCheckResult.JaveExeDoesNotExists:
                    {
                        _appCore.MessageService.Error("JavaPathExeDoesNotExists", true);
                        break;
                    }
                case JavaHelper.JavaPathCheckResult.WrongExe:
                    {
                        _appCore.MessageService.Error("JavaPathWrongExe", true);
                        break;
                    }
                case JavaHelper.JavaPathCheckResult.PathDoesNotExists:
                    {
                        _appCore.MessageService.Error("JavaPathPathDoesNotExists", true);
                        break;
                    }
            }
        }


        #endregion Private Methods
    }
}

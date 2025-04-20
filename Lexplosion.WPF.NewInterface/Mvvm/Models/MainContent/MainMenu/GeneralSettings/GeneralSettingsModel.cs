using Lexplosion.Core.Tools;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Tools;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Tools;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Windows.Forms;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.Content.GeneralSettings
{
    public sealed class GeneralSettingsModel : ObservableModelBase
    {
        public static event Action<bool> ConsoleParameterChanged;

        public override event Action<object> Notify;

        private readonly AppCore _appCore;
        private readonly ComputerInfo ci = new ComputerInfo();

        public IEnumerable<string> Resolutions { get; }


        #region Resolution


        private string _selectedResolution;
        public string SelectedResolution
        {
            get => _selectedResolution; set
            {
                _selectedResolution = value;
                OnPropertyChanged();
                OnResolutionChanged();
            }
        }


        #endregion Resolution


        #region Ram


        public ulong TotalPhysicalMemoryMb { get; }

        public ulong RamStep { get; }


        #endregion Ram


        #region Properties


        public string SystemPath
        {
            get => GlobalData.GeneralSettings.GamePath.Replace('\\', '/'); set
            {
                var isCorrectPath = DirectoryHelper.DirectoryNameIsValid(value);

                if (isCorrectPath)
                {
                    GlobalData.GeneralSettings.GamePath = WithDirectory.CreateAcceptableGamePath(value, out var _);
                    OnPropertyChanged();
                    DataFilesManager.SaveSettings(GlobalData.GeneralSettings);

                    _appCore.ModalNavigationStore.Open(
                        new ConfirmActionViewModel(
                            _appCore.Resources("DirectoryTransfer") as string,
                            string.Format(_appCore.Resources("DirectoryTransferDescription") as string),
                            _appCore.Resources("DirectoryTransferAgreeButtonText") as string,
                            (obj) =>
                            {
                                DirectoryTransfer();
                            })
                    );
                }
            }
        }

        public uint WindowHeight
        {
            get => (uint)GlobalData.GeneralSettings.WindowHeight; set
            {
                GlobalData.GeneralSettings.WindowHeight = value;
                OnPropertyChanged();
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }

        public uint WindowWidth
        {
            get => (uint)GlobalData.GeneralSettings.WindowWidth; set
            {
                GlobalData.GeneralSettings.WindowWidth = value;
                OnPropertyChanged();
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }

        public uint Xmx
        {
            get => (uint)GlobalData.GeneralSettings.Xmx; set
            {
                GlobalData.GeneralSettings.Xmx = value;
                OnPropertyChanged();
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }

        public uint Xms
        {
            get => (uint)GlobalData.GeneralSettings.Xms; set
            {
                GlobalData.GeneralSettings.Xms = value;
                OnPropertyChanged();
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }

        public bool? IsShowConsole
        {
            get => GlobalData.GeneralSettings.IsShowConsole; set
            {
                GlobalData.GeneralSettings.IsShowConsole = value;
                OnPropertyChanged();
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);

                ConsoleParameterChanged?.Invoke(value == true);
            }
        }

        public bool? IsHiddenMode
        {
            get => GlobalData.GeneralSettings.IsHiddenMode; set
            {
                GlobalData.GeneralSettings.IsHiddenMode = value;
                OnPropertyChanged();
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }

        public bool? IsAutoUpdate
        {
            get => GlobalData.GeneralSettings.IsAutoUpdate; set
            {
                GlobalData.GeneralSettings.IsAutoUpdate = value;
                OnPropertyChanged();
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }

        public bool? IsNightWorldSkinSystemEnabled
        {
            get => GlobalData.GeneralSettings.IsNightWorldSkinSystem; set
            {
                GlobalData.GeneralSettings.IsNightWorldSkinSystem = value;
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
                OnPropertyChanged();
            }
        }

        public bool? IsNightWorldClientEnabled
        {
            get => GlobalData.GeneralSettings.NwClientByDefault; set
            {
                GlobalData.GeneralSettings.NwClientByDefault = value;
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
                OnPropertyChanged();
            }
        }

        public string JavaPath
        {
            get => GlobalData.GeneralSettings.JavaPath; set
            {
                var javaPathResult = JavaHelper.TryValidateJavaPath(value, out value);

                if (javaPathResult == JavaHelper.JavaPathCheckResult.Success)
                {
                    GlobalData.GeneralSettings.JavaPath = value;
                    GlobalData.GeneralSettings.IsCustomJava = true;
                }
                else if (javaPathResult == JavaHelper.JavaPathCheckResult.EmptyOrNull)
                {
                    GlobalData.GeneralSettings.JavaPath = value;
                    GlobalData.GeneralSettings.IsCustomJava = false;
                }

                Notify?.Invoke(javaPathResult);
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
                OnPropertyChanged();
            }
        }


        public string Java17Path
        {
            get => GlobalData.GeneralSettings.Java17Path; set
            {
                var javaPathResult = JavaHelper.TryValidateJavaPath(value, out value);

                if (javaPathResult == JavaHelper.JavaPathCheckResult.Success)
                {
                    GlobalData.GeneralSettings.Java17Path = value;
                    GlobalData.GeneralSettings.IsCustomJava17 = false;
                }
                else if (javaPathResult == JavaHelper.JavaPathCheckResult.EmptyOrNull)
                {
                    GlobalData.GeneralSettings.Java17Path = value;
                    GlobalData.GeneralSettings.IsCustomJava17 = true;
                }

                Notify?.Invoke(javaPathResult);
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
                OnPropertyChanged();
            }
        }

        public string MinecraftArgs
        {
            get => GlobalData.GeneralSettings.GameArgs; set
            {
                GlobalData.GeneralSettings.GameArgs = value;
                OnPropertyChanged();
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }

        public string JVMArgs
        {
            get => GlobalData.GeneralSettings.JVMArgs; set
            {
                GlobalData.GeneralSettings.JVMArgs = value;
                OnPropertyChanged();
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }

        public void ResetJavaPath()
        {
            JavaPath = "";
        }


        public void ResetJava17Path()
        {
            Java17Path = "";
        }

        public void ResetGameDirectory()
        {
            SystemPath = LaunсherSettings.gamePath;
        }


        #endregion Properties


        #region Constructors


        public GeneralSettingsModel()
        {
            Resolutions = WindowsResolutionTools.GetAvaliableResolutionsToString();
            TotalPhysicalMemoryMb = (ci.TotalPhysicalMemory) / (1 << 20);
            RamStep = TotalPhysicalMemoryMb / 16;
        }


        #endregion Constructors


        #region Public Methods


        public void OpenGameDirectoryFolderBrowser()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = Java17Path.Replace('/', '\\');
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                }
            }
        }

        public void OpenJavaFolderBrowser()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = JavaPath.Replace('/', '\\');
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    JavaPath = dialog.SelectedPath;
                }
            }
        }

        public void OpenJava17FolderBrowser()
        {
            // TODO: может написать свой FolderBrowserDialog используя winforms и наследуясь от CommonDialog
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = Java17Path.Replace('/', '\\');
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Java17Path = dialog.SelectedPath;
                }
            }
        }


        #endregion Public Methods


        #region Private Methods


        private void OnResolutionChanged()
        {
            var resValues = SelectedResolution.Split('x');
            WindowWidth = uint.Parse(resValues[0]);
            WindowHeight = uint.Parse(resValues[1]);
        }


        private bool _isDirectoryTransferring;
        public bool IsDirectoryTransferring
        {
            get => _isDirectoryTransferring; set
            {
                _isDirectoryTransferring = value;
                OnPropertyChanged();
            }
        }

        private void DirectoryTransfer()
        {
            _appCore.SetGlobalLoadingStatus(true, "DirectoryTransferLoading", true);

            Runtime.TaskRun(() =>
            {
                WithDirectory.SetNewDirectory(SystemPath);
                _appCore.MessageService.Success("SuccessDirectoryTransfer", true);
                _appCore.SetGlobalLoadingStatus(false);
            });
        }

        #endregion Private Methods
    }
}

using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Tools;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.Content.GeneralSettings
{
    public sealed class GeneralSettingsModel : ViewModelBase
    {
        public static event Action<bool> ConsoleParameterChanged;

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
                GlobalData.GeneralSettings.GamePath = value.Replace('\\', '/');
                OnPropertyChanged();
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
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

        public string JavaPath
        {
            get => GlobalData.GeneralSettings.JavaPath; set
            {
                GlobalData.GeneralSettings.JavaPath = value;
                OnPropertyChanged();

                if (value.Length == 0)
                    GlobalData.GeneralSettings.IsCustomJava = false;
                else
                    GlobalData.GeneralSettings.IsCustomJava = true;

                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }


        public string Java17Path
        {
            get => GlobalData.GeneralSettings.Java17Path; set
            {
                GlobalData.GeneralSettings.Java17Path = value;
                OnPropertyChanged();

                if (value.Length == 0)
                {
                    GlobalData.GeneralSettings.IsCustomJava17 = false;
                }
                else
                {
                    GlobalData.GeneralSettings.IsCustomJava17 = true;
                }

                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }

        public string JVMArgs
        {
            get => GlobalData.GeneralSettings.GameArgs; set
            {
                GlobalData.GeneralSettings.GameArgs = value;
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
                    Java17Path = dialog.SelectedPath;
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


        #endregion Private Methods
    }
}

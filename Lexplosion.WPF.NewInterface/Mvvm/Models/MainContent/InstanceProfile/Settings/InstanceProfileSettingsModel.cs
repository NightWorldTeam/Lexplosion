using Lexplosion.Core.Tools;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Tools;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.InstanceProfile.Settings
{
    public sealed class InstanceProfileSettingsModel : ObservableModelBase
    {
        public static event Action<bool, string> ConsoleParameterChanged;

        public override event Action<object> Notify;

        private InstanceModelBase _instanceModel;
        private Lexplosion.Logic.Settings _instanceSettings;
        private Lexplosion.Logic.Settings _instanceSettingsCopy;

        private readonly ComputerInfo ci = new ComputerInfo();


        #region Properties


        public Lexplosion.Logic.Settings InstanceSettings
        {
            get => _instanceSettings; set
            {
                _instanceSettings = value;
                OnPropertyChanged();
            }
        }


        #region WindowProperties


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

        public IEnumerable<string> Resolutions { get; }


        public uint WindowHeight
        {
            get => (uint)(InstanceSettings.WindowHeight); set
            {
                InstanceSettings.WindowHeight = value;
                _instanceSettingsCopy.WindowHeight = value;
                OnPropertyChanged();
                _instanceModel.Settings = _instanceSettingsCopy;
            }
        }

        public uint WindowWidth
        {
            get => (uint)(InstanceSettings.WindowWidth); set
            {
                InstanceSettings.WindowWidth = value;
                _instanceSettingsCopy.WindowWidth = value;
                OnPropertyChanged();
                _instanceModel.Settings = _instanceSettingsCopy;
            }
        }

        public string ScreenResolution
        {
            get => WindowWidth.ToString() + "x" + WindowHeight.ToString(); set
            {
                var resolution = value.Split('x');

                WindowWidth = uint.Parse(resolution[0]);
                WindowHeight = uint.Parse(resolution[1]);
            }
        }


        #endregion WindowProperties


        #region RamProperties


        public ulong TotalPhysicalMemoryMb { get; }

        public ulong RamStep { get; }


        public uint Xmx
        {
            get => (uint)InstanceSettings.Xmx; set
            {
                InstanceSettings.Xmx = value;
                _instanceSettingsCopy.Xmx = value;
                OnPropertyChanged();
                _instanceModel.Settings = _instanceSettingsCopy;
            }
        }

        public uint Xms
        {
            get => (uint)InstanceSettings.Xms; set
            {
                InstanceSettings.Xms = value;
                _instanceSettingsCopy.Xms = value;
                OnPropertyChanged();
                _instanceModel.Settings = _instanceSettingsCopy;
            }
        }


        #endregion RamProperties


        #region Java Properties


        public string JavaPath
        {
            get => InstanceSettings.JavaPath; set
            {
                var javaPathResult = JavaHelper.TryValidateJavaPath(value, out value);

                if (javaPathResult == JavaHelper.JavaPathCheckResult.Success)
                {
                    InstanceSettings.JavaPath = value;
                    _instanceSettingsCopy.JavaPath = value;
                    InstanceSettings.IsCustomJava = true;
                }
                else if (javaPathResult == JavaHelper.JavaPathCheckResult.EmptyOrNull)
                {
                    InstanceSettings.JavaPath = value;
                    _instanceSettingsCopy.JavaPath = value;
                    InstanceSettings.IsCustomJava = false;
                }

                Notify?.Invoke(javaPathResult);
                _instanceModel.Settings = _instanceSettingsCopy;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// Аргументы для Minecraft
        /// </summary>
        public string MinecraftArgs
        {
            get => InstanceSettings.GameArgs; set
            {
                InstanceSettings.GameArgs = value;
                _instanceSettingsCopy.JavaPath = value;
                _instanceModel.Settings = _instanceSettingsCopy;
            }
        }

        /// <summary>
        /// Аргументы для JVM
        /// </summary>
        public string JVMArgs
        {
            get => InstanceSettings.JVMArgs; set 
            {
                InstanceSettings.JVMArgs = value;
                _instanceSettingsCopy.JVMArgs = value;
                _instanceModel.Settings = _instanceSettingsCopy;
            }
        }


        #endregion Java Properties


        #region Launcher Properties


        public bool IsShowConsole
        {
            get => (bool)InstanceSettings.IsShowConsole; set
            {
                InstanceSettings.IsShowConsole = value;
                _instanceSettingsCopy.IsShowConsole = value;
                OnPropertyChanged();
                _instanceModel.Settings = _instanceSettingsCopy;

                ConsoleParameterChanged?.Invoke(value == true, _instanceModel.LocalId);
            }
        }

        public bool IsHiddenMode
        {
            get => (bool)InstanceSettings.IsHiddenMode; set
            {
                InstanceSettings.IsHiddenMode = value;
                _instanceSettingsCopy.IsHiddenMode = value;
                OnPropertyChanged();
                _instanceModel.Settings = _instanceSettingsCopy;
            }
        }

        public bool IsAutoUpdate
        {
            get => (bool)InstanceSettings.IsAutoUpdate; set
            {
                InstanceSettings.IsAutoUpdate = value;
                _instanceSettingsCopy.IsAutoUpdate = value;
                OnPropertyChanged();
                _instanceModel.Settings = _instanceSettingsCopy;
            }
        }

        public bool? IsNightWorldSkinSystemEnabled
        {
            get => InstanceSettings.IsNightWorldSkinSystem; set
            {
                InstanceSettings.IsNightWorldSkinSystem = value;
                _instanceSettingsCopy.IsAutoUpdate = value;
                _instanceModel.Settings = _instanceSettingsCopy;
                OnPropertyChanged();
            }
        }


        #endregion Launcher Properties


        #endregion Properties


        #region Constructors


        public InstanceProfileSettingsModel(InstanceModelBase instanceModel)
        {
            _instanceModel = instanceModel;
            _instanceSettings = instanceModel.Settings;
            _instanceSettingsCopy = _instanceSettings.Copy();
            InstanceSettings.Merge(GlobalData.GeneralSettings, true);

            Resolutions = WindowsResolutionTools.GetAvaliableResolutionsToString();
            TotalPhysicalMemoryMb = (ci.TotalPhysicalMemory) / (1 << 20);
            RamStep = TotalPhysicalMemoryMb / 16;
        }


        #endregion Constructors


        #region Public Methods


        public void OpenJavaFolderBrowser(object obj)
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


        public void ResetJavaPath(object obj) { }


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

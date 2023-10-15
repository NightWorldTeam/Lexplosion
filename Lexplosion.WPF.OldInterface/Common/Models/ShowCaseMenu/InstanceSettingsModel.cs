using Lexplosion.Controls;
using Lexplosion.Core.Tools;
using Lexplosion.Global;
using Lexplosion.Logic;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Tools;
using System;

namespace Lexplosion.Common.Models.ShowCaseMenu
{
    public class InstanceSettingsModel : VMBase
    {
        private InstanceClient _instanceClient;
        private Settings _instanceSettings;
        private Settings _instanceSettingsCopy;

        public static event Action<bool, string> ConsoleParameterChanged;

        public DoNotificationCallback _doNotificationCallback = (a, a1, a2, a3) => { };

        #region Properties


        public Settings InstanceSettings
        {
            get => _instanceSettings; set
            {
                _instanceSettings = value;
                OnPropertyChanged();
            }
        }

        public uint WindowHeight
        {
            get => (uint)(InstanceSettings.WindowHeight); set
            {
                InstanceSettings.WindowHeight = value;
                _instanceSettingsCopy.WindowHeight = value;
                OnPropertyChanged();
                _instanceClient.SaveSettings(_instanceSettingsCopy);
            }
        }

        public uint WindowWidth
        {
            get => (uint)(InstanceSettings.WindowWidth); set
            {
                InstanceSettings.WindowWidth = value;
                _instanceSettingsCopy.WindowWidth = value;
                OnPropertyChanged();
                _instanceClient.SaveSettings(_instanceSettingsCopy);
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

        public uint Xmx
        {
            get => (uint)InstanceSettings.Xmx; set
            {
                InstanceSettings.Xmx = value;
                _instanceSettingsCopy.Xmx = value;
                OnPropertyChanged();
                _instanceClient.SaveSettings(_instanceSettingsCopy);
            }
        }

        public uint Xms
        {
            get => (uint)InstanceSettings.Xms; set
            {
                InstanceSettings.Xms = value;
                _instanceSettingsCopy.Xms = value;
                OnPropertyChanged();
                _instanceClient.SaveSettings(_instanceSettingsCopy);
            }
        }

        public string GameArgs
        {
            get => InstanceSettings.GameArgs; set
            {
                InstanceSettings.GameArgs = value;
                _instanceSettingsCopy.GameArgs = value;
                OnPropertyChanged();
                _instanceClient.SaveSettings(_instanceSettingsCopy);
            }
        }

        public bool IsAutoUpdate
        {
            get => (bool)InstanceSettings.IsAutoUpdate; set
            {
                InstanceSettings.IsAutoUpdate = value;
                _instanceSettingsCopy.IsAutoUpdate = value;
                OnPropertyChanged();
                _instanceClient.SaveSettings(_instanceSettingsCopy);
            }
        }

        public string JavaPath
        {
            get => InstanceSettings.JavaPath; set
            {
                var javaPathResult = JavaHelper.TryValidateJavaPath(value, out var correctPath);

                if (javaPathResult == JavaHelper.JavaPathCheckResult.Success)
                {
                    InstanceSettings.JavaPath = correctPath;
                    InstanceSettings.IsCustomJava = true;
                }
                else
                {
                    DoErrorNotification(javaPathResult);
                    InstanceSettings.JavaPath = string.Empty;
                    InstanceSettings.IsCustomJava = false;
                }

                OnPropertyChanged();

                DataFilesManager.SaveSettings(InstanceSettings);
            }
        }

        public string JVMArgs
        {
            get => InstanceSettings.GameArgs; set
            {
                InstanceSettings.GameArgs = value;
                _instanceSettingsCopy.JavaPath = value;
                OnPropertyChanged();
                _instanceClient.SaveSettings(_instanceSettingsCopy);
            }
        }

        public bool IsHiddenMode
        {
            get => (bool)InstanceSettings.IsHiddenMode; set
            {
                InstanceSettings.IsHiddenMode = value;
                _instanceSettingsCopy.IsHiddenMode = value;
                OnPropertyChanged();
                _instanceClient.SaveSettings(_instanceSettingsCopy);
            }
        }

        public bool IsShowConsole
        {
            get => (bool)InstanceSettings.IsShowConsole; set
            {
                InstanceSettings.IsShowConsole = value;
                _instanceSettingsCopy.IsShowConsole = value;
                OnPropertyChanged();
                _instanceClient.SaveSettings(_instanceSettingsCopy);

                ConsoleParameterChanged?.Invoke(value == true, _instanceClient.LocalId);
            }
        }


        #endregion Properties


        #region Constructors


        public InstanceSettingsModel(InstanceClient instanceClient, DoNotificationCallback doNotificationCallback)
        {
            _doNotificationCallback = doNotificationCallback ?? _doNotificationCallback;
            _instanceClient = instanceClient;
            _instanceSettings = instanceClient.GetSettings();
            _instanceSettingsCopy = _instanceSettings.Copy();
            InstanceSettings.Merge(GlobalData.GeneralSettings, true);
        }


        #endregion Constructors


        #region Private Methods


        private void DoErrorNotification(JavaHelper.JavaPathCheckResult javaPathCheckResult)
        {
            switch (javaPathCheckResult)
            {
                case JavaHelper.JavaPathCheckResult.EmptyOrNull:
                    {
                        _doNotificationCallback(ResourceGetter.GetString("javaPathSelectError"), ResourceGetter.GetString("emptyOrNull"), 10, 0);
                        break;
                    }
                case JavaHelper.JavaPathCheckResult.JaveExeDoesNotExists:
                    {
                        _doNotificationCallback(ResourceGetter.GetString("javaPathSelectError"), ResourceGetter.GetString("javeExeDoesNotExists"), 10, 0);
                        break;
                    }
                case JavaHelper.JavaPathCheckResult.WrongExe:
                    {
                        _doNotificationCallback(ResourceGetter.GetString("javaPathSelectError"), ResourceGetter.GetString("wrongExe"), 10, 0);
                        break;
                    }
                case JavaHelper.JavaPathCheckResult.PathDoesNotExists:
                    {
                        _doNotificationCallback(ResourceGetter.GetString("javaPathSelectError"), ResourceGetter.GetString("pathDoesNotExists"), 10, 0);
                        break;
                    }
            }
        }


        #endregion Private Methods
    }
}

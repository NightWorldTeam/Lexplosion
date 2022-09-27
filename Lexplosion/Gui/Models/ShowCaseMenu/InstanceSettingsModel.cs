using Lexplosion.Global;
using Lexplosion.Logic;
using Lexplosion.Logic.Management.Instances;

namespace Lexplosion.Gui.Models.ShowCaseMenu
{
    public class InstanceSettingsModel : VMBase
    {
        private InstanceClient _instanceClient;
        private Settings _instanceSettings;
        private Settings _instanceSettingsCopy;

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
                var resolution = value.ToString().Split('x');

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

        public bool? IsAutoUpdate
        {
            get => InstanceSettings.AutoUpdate; set
            {
                InstanceSettings.AutoUpdate = value;
                _instanceSettingsCopy.AutoUpdate = value;
                OnPropertyChanged();
                _instanceClient.SaveSettings(_instanceSettingsCopy);
            }
        }

        public string JavaPath
        {
            get => InstanceSettings.JavaPath; set
            {
                InstanceSettings.JavaPath = value;
                _instanceSettingsCopy.JavaPath = value;
                OnPropertyChanged();

                if (value.Length == 0)
                    InstanceSettings.CustomJava = false;
                else
                    InstanceSettings.CustomJava = true;

                _instanceClient.SaveSettings(_instanceSettingsCopy);
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

        public InstanceSettingsModel(InstanceClient instanceClient)
        {
            _instanceClient = instanceClient;
            _instanceSettings = instanceClient.GetSettings();
            _instanceSettingsCopy = _instanceSettings.Copy();
            InstanceSettings.Merge(UserData.GeneralSettings, true);
        }
    }
}

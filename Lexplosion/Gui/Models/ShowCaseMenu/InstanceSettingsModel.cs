using Lexplosion.Global;
using Lexplosion.Logic;
using Lexplosion.Logic.Management.Instances;

namespace Lexplosion.Gui.Models.ShowCaseMenu
{
    public class InstanceSettingsModel : VMBase
    {
        private InstanceClient _instanceClient;
        private Settings _instanceSettings;

        public Settings InstanceSettings 
        {
            get => _instanceSettings; set
            {
                _instanceSettings = value;
                OnPropertyChanged(nameof(_instanceSettings));
            }
        }

        public uint WindowHeight
        {
            get => (uint)(InstanceSettings.WindowHeight); set
            {
                InstanceSettings.WindowHeight = value;
                OnPropertyChanged("WindowHeight");
                _instanceClient.SaveSettings(InstanceSettings);
            }
        }

        public uint WindowWidth
        {
            get => (uint)(InstanceSettings.WindowWidth); set
            {
                InstanceSettings.WindowWidth = value;
                OnPropertyChanged("WindowWidth");
                _instanceClient.SaveSettings(InstanceSettings);
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
                OnPropertyChanged("Xmx");
                _instanceClient.SaveSettings(InstanceSettings);
            }
        }

        public uint Xms
        {
            get => (uint)InstanceSettings.Xms; set
            {
                InstanceSettings.Xms = value;
                OnPropertyChanged("Xms");
                _instanceClient.SaveSettings(InstanceSettings);
            }
        }

        public string GameArgs
        {
            get => InstanceSettings.GameArgs; set
            {
                InstanceSettings.GameArgs = value;
                OnPropertyChanged("GameArgs");
                _instanceClient.SaveSettings(InstanceSettings);
            }
        }

        public bool? IsAutoUpdate
        {
            get => InstanceSettings.AutoUpdate; set
            {
                InstanceSettings.AutoUpdate = value;
                OnPropertyChanged("IsAutoUpdate");
                _instanceClient.SaveSettings(InstanceSettings);
            }
        }

        public string JavaPath
        {
            get => InstanceSettings.JavaPath; set
            {
                InstanceSettings.JavaPath = value;
                OnPropertyChanged();

                if (value.Length == 0)
                    InstanceSettings.CustomJava = false;
                else
                    InstanceSettings.CustomJava = true;

                _instanceClient.SaveSettings(InstanceSettings);
            }
        }

        public string JVMArgs
        {
            get => InstanceSettings.GameArgs; set
            {
                InstanceSettings.GameArgs = value;
                OnPropertyChanged();
                _instanceClient.SaveSettings(InstanceSettings);
            }
        }

        public InstanceSettingsModel(InstanceClient instanceClient)
        {
            _instanceClient = instanceClient;
            _instanceSettings = instanceClient.GetSettings();
            InstanceSettings.Merge(UserData.GeneralSettings, true);
        }
    }
}

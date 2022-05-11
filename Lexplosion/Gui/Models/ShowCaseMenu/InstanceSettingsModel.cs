using Lexplosion.Global;
using Lexplosion.Logic;
using Lexplosion.Logic.FileSystem;
using System.Collections.Generic;

namespace Lexplosion.Gui.Models.ShowCaseMenu
{
    public class InstanceSettingsModel : VMBase
    {
        private Settings _instanceSettings;
        private string _instanceId;

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
                DataFilesManager.SaveSettings(InstanceSettings, _instanceId);
            }
        }

        public uint WindowWidth
        {
            get => (uint)(InstanceSettings.WindowWidth); set
            {
                InstanceSettings.WindowWidth = value;
                OnPropertyChanged("WindowWidth");
                DataFilesManager.SaveSettings(InstanceSettings, _instanceId);
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
                DataFilesManager.SaveSettings(InstanceSettings, _instanceId);
            }
        }

        public uint Xms
        {
            get => (uint)InstanceSettings.Xms; set
            {
                InstanceSettings.Xms = value;
                OnPropertyChanged("Xms");
                DataFilesManager.SaveSettings(InstanceSettings, _instanceId);
            }
        }

        public string JavaPath
        {
            get => InstanceSettings.JavaPath; set
            {
                InstanceSettings.JavaPath = value;
                OnPropertyChanged("JavaPath");
                DataFilesManager.SaveSettings(InstanceSettings, _instanceId);
            }
        }

        public string GameArgs
        {
            get => InstanceSettings.GameArgs; set
            {
                InstanceSettings.GameArgs = value;
                OnPropertyChanged("GameArgs");
                DataFilesManager.SaveSettings(InstanceSettings, _instanceId);
            }
        }

        public bool? IsAutoUpdate
        {
            get => InstanceSettings.AutoUpdate; set
            {
                InstanceSettings.AutoUpdate = value;
                OnPropertyChanged("IsAutoUpdate");
                DataFilesManager.SaveSettings(InstanceSettings, _instanceId);
            }
        }

        public InstanceSettingsModel(string instanceId)
        {
            _instanceSettings = DataFilesManager.GetSettings(instanceId);
            InstanceSettings.Merge(UserData.GeneralSettings, true);

            _instanceId = instanceId;
        }
    }
}

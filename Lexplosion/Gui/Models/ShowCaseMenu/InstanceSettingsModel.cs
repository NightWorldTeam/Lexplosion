using Lexplosion.Global;
using Lexplosion.Logic;
using Lexplosion.Logic.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.Models.ShowCaseMenu
{
    public class InstanceSettingsModel : VMBase
    {
        public readonly static List<string> ScreenResolutions = new List<string>()
        {
            "1920x1080", "1768x992", "1680x1050",  "1600x1024", "1600x900", "1440x900", "1280x1024",
            "1280x960", "1366x768", "1360x768", "1280x800", "1280x768", "1152x864", "1280x720", "1176x768",
            "1024x768", "800x600", "720x576", "720x480", "640x480"
        };

        private Settings _instanceSettings;
        private Settings SettingsCopied { get; }
        private string _instanceId;

        public List<string> Resolutions
        {
            get => ScreenResolutions;
        }

        public Settings InstanceSettings 
        {
            get => _instanceSettings; set
            {
                _instanceSettings = value;
                OnPropertyChanged(nameof(_instanceSettings));
            }
        }

        public string SystemPath
        {
            get => InstanceSettings.GamePath.Replace(@"\", "/"); set
            {
                InstanceSettings.GamePath = value.Replace(@"\", "/");
                OnPropertyChanged("SystemPath");
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
            SettingsCopied = _instanceSettings.Copy();
            InstanceSettings.Merge(UserData.GeneralSettings, true);

            _instanceId = instanceId;
        }
    }
}

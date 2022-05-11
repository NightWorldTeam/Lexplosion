using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using System.Collections.Generic;

namespace Lexplosion.Gui.Models
{
    public class GeneralSettingsModel : VMBase
    {
        public string SystemPath
        {
            get => UserData.GeneralSettings.GamePath.Replace(@"\", "/"); set
            {
                UserData.GeneralSettings.GamePath = value.Replace(@"\", "/");
                OnPropertyChanged("SystemPath");
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
            }
        }

        public uint WindowHeight
        {
            get => (uint)UserData.GeneralSettings.WindowHeight; set
            {
                UserData.GeneralSettings.WindowHeight = value;
                OnPropertyChanged("WindowHeight");
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
            }
        }

        public uint WindowWidth
        {
            get => (uint)UserData.GeneralSettings.WindowWidth; set
            {
                UserData.GeneralSettings.WindowWidth = value;
                OnPropertyChanged("WindowWidth");
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
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
            get => (uint)UserData.GeneralSettings.Xmx; set
            {
                UserData.GeneralSettings.Xmx = value;
                OnPropertyChanged("Xmx");
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
            }
        }

        public uint Xms
        {
            get => (uint)UserData.GeneralSettings.Xms; set
            {
                UserData.GeneralSettings.Xms = value;
                OnPropertyChanged("Xms");
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
            }
        }

        public bool? IsShowConsole
        {
            get => UserData.GeneralSettings.ShowConsole; set
            {
                UserData.GeneralSettings.ShowConsole = value;
                OnPropertyChanged("IsShowConsole");
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
            }
        }

        public string GameArgs
        {
            get => UserData.GeneralSettings.GameArgs; set
            {
                UserData.GeneralSettings.GameArgs = value;
                OnPropertyChanged("GameArgs");
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
            }
        }

        public bool? IsHiddenMode
        {
            get => UserData.GeneralSettings.HiddenMode; set
            {
                UserData.GeneralSettings.HiddenMode = value;
                OnPropertyChanged("IsHiddenMode");
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
            }
        }

        public bool? IsAutoUpdate
        {
            get => UserData.GeneralSettings.AutoUpdate; set
            {
                UserData.GeneralSettings.AutoUpdate = value;
                OnPropertyChanged("IsAutoUpdate");
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
            }
        }
    }
}

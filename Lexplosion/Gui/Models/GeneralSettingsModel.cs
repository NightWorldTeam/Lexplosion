using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;

namespace Lexplosion.Gui.Models
{
    public class GeneralSettingsModel : VMBase
    {
        public string SystemPath
        {
            get => UserData.GeneralSettings.GamePath.Replace(@"\", "/"); set
            {
                UserData.GeneralSettings.GamePath = value.Replace(@"\", "/");
                OnPropertyChanged();
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
            }
        }

        public uint WindowHeight
        {
            get => (uint)UserData.GeneralSettings.WindowHeight; set
            {
                UserData.GeneralSettings.WindowHeight = value;
                OnPropertyChanged();
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
            }
        }

        public uint WindowWidth
        {
            get => (uint)UserData.GeneralSettings.WindowWidth; set
            {
                UserData.GeneralSettings.WindowWidth = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
            }
        }

        public uint Xms
        {
            get => (uint)UserData.GeneralSettings.Xms; set
            {
                UserData.GeneralSettings.Xms = value;
                OnPropertyChanged();
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
            }
        }

        public string GameArgs
        {
            get => UserData.GeneralSettings.GameArgs; set
            {
                UserData.GeneralSettings.GameArgs = value;
                OnPropertyChanged();
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
            }
        }

        public bool? IsShowConsole
        {
            get => UserData.GeneralSettings.ShowConsole; set
            {
                UserData.GeneralSettings.ShowConsole = value;
                OnPropertyChanged();
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
            }
        }

        public bool? IsHiddenMode
        {
            get => UserData.GeneralSettings.HiddenMode; set
            {
                UserData.GeneralSettings.HiddenMode = value;
                OnPropertyChanged();
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
            }
        }

        public bool? IsAutoUpdate
        {
            get => UserData.GeneralSettings.AutoUpdate; set
            {
                UserData.GeneralSettings.AutoUpdate = value;
                OnPropertyChanged();
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
            }
        }

        public string JavaPath 
        {
            get => UserData.GeneralSettings.JavaPath; set 
            {
                UserData.GeneralSettings.JavaPath = value;
                OnPropertyChanged();

                if (value.Length == 0)
                    UserData.GeneralSettings.CustomJava = false;
                else 
                    UserData.GeneralSettings.CustomJava = true;

                DataFilesManager.SaveSettings(UserData.GeneralSettings);
            }
        }

        public string JVMArgs
        {
            get => UserData.GeneralSettings.GameArgs; set
            {
                UserData.GeneralSettings.GameArgs = value;
                OnPropertyChanged();
                DataFilesManager.SaveSettings(UserData.GeneralSettings);
            }
        }
    }
}

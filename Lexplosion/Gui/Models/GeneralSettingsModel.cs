using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;

namespace Lexplosion.Gui.Models
{
    public class GeneralSettingsModel : VMBase
    {
        public string SystemPath
        {
            get => GlobalData.GeneralSettings.GamePath.Replace(@"\", "/"); set
            {
                GlobalData.GeneralSettings.GamePath = value.Replace(@"\", "/");
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

        public string GameArgs
        {
            get => GlobalData.GeneralSettings.GameArgs; set
            {
                GlobalData.GeneralSettings.GameArgs = value;
                OnPropertyChanged();
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }

        public bool? IsShowConsole
        {
            get => GlobalData.GeneralSettings.ShowConsole; set
            {
                GlobalData.GeneralSettings.ShowConsole = value;
                OnPropertyChanged();
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }

        public bool? IsHiddenMode
        {
            get => GlobalData.GeneralSettings.HiddenMode; set
            {
                GlobalData.GeneralSettings.HiddenMode = value;
                OnPropertyChanged();
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }

        public bool? IsAutoUpdate
        {
            get => GlobalData.GeneralSettings.AutoUpdate; set
            {
                GlobalData.GeneralSettings.AutoUpdate = value;
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
                    GlobalData.GeneralSettings.CustomJava = false;
                else
                    GlobalData.GeneralSettings.CustomJava = true;

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
    }
}

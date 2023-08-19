using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using System;

namespace Lexplosion.Common.Models
{
    public sealed class GeneralSettingsModel : VMBase
    {
        public static event Action<bool> ConsoleParameterChanged;

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
    }
}

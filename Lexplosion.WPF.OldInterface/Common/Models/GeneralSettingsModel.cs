using Lexplosion.Controls;
using Lexplosion.Core.Tools;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Tools;
using System;
using System.Diagnostics.Eventing.Reader;

namespace Lexplosion.Common.Models
{
    public sealed class GeneralSettingsModel : VMBase
    {
        public static event Action<bool> ConsoleParameterChanged;

        private readonly DoNotificationCallback _doNotification = (header, message, time, type) => { };

        private bool _isJavaPathWasReseted;

        #region Public Methods


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
                if (!_isJavaPathWasReseted)
                {
                    var javaPathResult = JavaHelper.TryValidateJavaPath(value, out value);

                    if (javaPathResult == JavaHelper.JavaPathCheckResult.Success)
                    {
                        GlobalData.GeneralSettings.JavaPath = value;
                        GlobalData.GeneralSettings.IsCustomJava = true;
                    }
                    else
                    {
                        DoErrorNotification(javaPathResult);
                        GlobalData.GeneralSettings.JavaPath = string.Empty;
                        GlobalData.GeneralSettings.IsCustomJava = false;
                    }
                }
                else
                {
                    GlobalData.GeneralSettings.JavaPath = value;
                }

                OnPropertyChanged();

                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }


        public string Java17Path
        {
            get => GlobalData.GeneralSettings.Java17Path; set
            {
                if (!_isJavaPathWasReseted)
                {
                    var javaPathResult = JavaHelper.TryValidateJavaPath(value, out value);

                    if (javaPathResult == JavaHelper.JavaPathCheckResult.Success)
                    {
                        GlobalData.GeneralSettings.Java17Path = value;
                        GlobalData.GeneralSettings.IsCustomJava17 = true;
                    }
                    else
                    {
                        DoErrorNotification(javaPathResult);
                        GlobalData.GeneralSettings.Java17Path = string.Empty;
                        GlobalData.GeneralSettings.IsCustomJava17 = false;
                    }
                }
                else 
                {
                    GlobalData.GeneralSettings.Java17Path = value;
                }

                OnPropertyChanged();

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
            _isJavaPathWasReseted = true;
            JavaPath = "";
            _isJavaPathWasReseted = false;
        }


        public void ResetJava17Path()
        {
            _isJavaPathWasReseted = true;
            Java17Path = "";
            _isJavaPathWasReseted = false;
        }


        #endregion Public Methods


        #region Constructors


        public GeneralSettingsModel(DoNotificationCallback doNotificationCallback)
        {
            _doNotification = doNotificationCallback;
        }


        #endregion Constructors


        #region Private Methods


        private void DoErrorNotification(JavaHelper.JavaPathCheckResult javaPathCheckResult) 
        {
            switch (javaPathCheckResult) 
            {
                case JavaHelper.JavaPathCheckResult.EmptyOrNull:
                    {
                        _doNotification(ResourceGetter.GetString("javaPathSelectError"), ResourceGetter.GetString("emptyOrNull"), 10, 0);
                        break;
                    }
                case JavaHelper.JavaPathCheckResult.JaveExeDoesNotExists:
                {
                    _doNotification(ResourceGetter.GetString("javaPathSelectError"), ResourceGetter.GetString("javeExeDoesNotExists"), 10, 0);
                    break;
                }
                case JavaHelper.JavaPathCheckResult.WrongExe:
                    {
                        _doNotification(ResourceGetter.GetString("javaPathSelectError"), ResourceGetter.GetString("wrongExe"), 10, 0);
                        break;
                    }
                case JavaHelper.JavaPathCheckResult.PathDoesNotExists:
                    {
                        _doNotification(ResourceGetter.GetString("javaPathSelectError"), ResourceGetter.GetString("pathDoesNotExists"), 10, 0);
                        break;
                    }
            }
        }


        #endregion Private Methods
    }
}

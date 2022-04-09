using Lexplosion.Global;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic
{
    public class Settings
    {
        public string JavaPath = null;
        public string GamePath = null;
        public uint? Xmx = null;
        public uint? Xms = null;
        public uint? WindowWidth = null;
        public uint? WindowHeight = null;
        public bool? ShowConsole = null;
        public bool? HiddenMode = null;
        public bool? AutoUpdate = null;
        public string GameArgs = null;

        public void Merge(Settings settings, bool priority = false)
        {
            if (priority)
            {
                if (JavaPath == null) JavaPath = settings.JavaPath;
                if (GamePath == null) GamePath = settings.GamePath;
                if (Xmx == null) Xmx = settings.Xmx;
                if (Xms == null) Xms = settings.Xms;
                if (WindowWidth == null) WindowWidth = settings.WindowWidth;
                if (WindowHeight == null) WindowHeight = settings.WindowHeight;
                if (ShowConsole == null) ShowConsole = settings.ShowConsole;
                if (HiddenMode == null) HiddenMode = settings.HiddenMode;
                if (AutoUpdate == null) AutoUpdate = settings.AutoUpdate;
                if (GameArgs == null) GameArgs = settings.GameArgs;
            }
            else
            {
                if (settings.JavaPath != null) JavaPath = settings.JavaPath;
                if (settings.GamePath != null) GamePath = settings.GamePath;
                if (settings.Xmx != null) Xmx = settings.Xmx;
                if (settings.Xms != null) Xms = settings.Xms;
                if (settings.WindowWidth != null) WindowWidth = settings.WindowWidth;
                if (settings.WindowHeight != null) WindowHeight = settings.WindowHeight;
                if (settings.ShowConsole != null) ShowConsole = settings.ShowConsole;
                if (settings.HiddenMode != null) HiddenMode = settings.HiddenMode;
                if (settings.AutoUpdate != null) AutoUpdate = settings.AutoUpdate;
                if (settings.GameArgs != null) GameArgs = settings.GameArgs;
            }
        }

        public static Settings GetDefault()
        {
            /*<!-- получение директории до джавы -->*/
            string javaPath = "";
            try
            {
                using (RegistryKey jre = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    RegistryKey java = jre.OpenSubKey(@"SOFTWARE\JavaSoft\Java Runtime Environment");
                    javaPath = (java.OpenSubKey(java.GetValue("CurrentVersion").ToString()).GetValue("JavaHome").ToString() + @"/bin/javaw.exe").Replace(@"\", "/");
                }

            }
            catch
            {
                // TODO: тут ставить автоматически установленную джаву
            }

            Settings settengs = new Settings
            {
                JavaPath = javaPath,
                GamePath = LaunсherSettings.gamePath,
                Xmx = 512,
                Xms = 256,
                WindowWidth = 854,
                WindowHeight = 480,
                ShowConsole = false,
                HiddenMode = false,
                GameArgs = ""
            };

            return settengs;
        }
    }
}

using Lexplosion.Global;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic
{
    [Serializable]
    public class Settings
    {
        public string JavaPath = null;
        public bool? CustomJava = null;
        public string GamePath = null;
        public uint? Xmx = null;
        public uint? Xms = null;
        public uint? WindowWidth = null;
        public uint? WindowHeight = null;
        public bool? ShowConsole = null;
        public bool? HiddenMode = null;
        public bool? AutoUpdate = null;
        public string GameArgs = null;
        /// <summary>
        /// Использовать ли в приоритете в сетевой игре прямое подключение.
        /// </summary>
        public bool OnlineGameDirectConnection = false;

        public void Merge(Settings settings, bool priority = false)
        {
            if (priority)
            {
                if (JavaPath == null) JavaPath = settings.JavaPath;
                if (CustomJava == null) CustomJava = settings.CustomJava;
                if (GamePath == null) GamePath = settings.GamePath;
                if (Xmx == null) Xmx = settings.Xmx;
                if (Xms == null) Xms = settings.Xms;
                if (WindowWidth == null) WindowWidth = settings.WindowWidth;
                if (WindowHeight == null) WindowHeight = settings.WindowHeight;
                if (ShowConsole == null) ShowConsole = settings.ShowConsole;
                if (HiddenMode == null) HiddenMode = settings.HiddenMode;
                if (AutoUpdate == null) AutoUpdate = settings.AutoUpdate;
                if (GameArgs == null) GameArgs = settings.GameArgs;
                if (OnlineGameDirectConnection == null) OnlineGameDirectConnection = settings.OnlineGameDirectConnection;
            }
            else
            {
                if (settings.JavaPath != null) JavaPath = settings.JavaPath;
                if (settings.CustomJava != null) CustomJava = settings.CustomJava;
                if (settings.GamePath != null) GamePath = settings.GamePath;
                if (settings.Xmx != null) Xmx = settings.Xmx;
                if (settings.Xms != null) Xms = settings.Xms;
                if (settings.WindowWidth != null) WindowWidth = settings.WindowWidth;
                if (settings.WindowHeight != null) WindowHeight = settings.WindowHeight;
                if (settings.ShowConsole != null) ShowConsole = settings.ShowConsole;
                if (settings.HiddenMode != null) HiddenMode = settings.HiddenMode;
                if (settings.AutoUpdate != null) AutoUpdate = settings.AutoUpdate;
                if (settings.GameArgs != null) GameArgs = settings.GameArgs;
                if (settings.OnlineGameDirectConnection != null) OnlineGameDirectConnection = settings.OnlineGameDirectConnection;
            }
        }

        public Settings Copy()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Position = 0;
                return (Settings)formatter.Deserialize(ms);
            }
        }

        public static Settings GetDefault()
        {
            return new Settings
            {
                JavaPath = "",
                CustomJava = false,
                GamePath = LaunсherSettings.gamePath,
                Xmx = 512,
                Xms = 256,
                WindowWidth = 854,
                WindowHeight = 480,
                ShowConsole = false,
                HiddenMode = false,
                GameArgs = "",
                AutoUpdate = false,
                OnlineGameDirectConnection = false
            };
        }
    }
}

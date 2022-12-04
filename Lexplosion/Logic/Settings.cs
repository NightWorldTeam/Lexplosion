using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Lexplosion.Global;
using Lexplosion.Tools;

namespace Lexplosion.Logic
{
    [Serializable]
    public class Settings
    {
        public string JavaPath = null;
        public bool? IsCustomJava = null;
        public string GamePath = null;
        public uint? Xmx = null;
        public uint? Xms = null;
        public uint? WindowWidth = null;
        public uint? WindowHeight = null;
        public bool? IsShowConsole = null;
        public bool? IsHiddenMode = null;
        public bool? IsAutoUpdate = null;
        public string GameArgs = null;
        /// <summary>
        /// Использовать ли в приоритете в сетевой игре прямое подключение.
        /// </summary>
        public bool OnlineGameDirectConnection = false;
        public string LanguageId = "";

        public void Merge(Settings settings, bool priority = false)
        {
            if (priority)
            {
                if (JavaPath == null) JavaPath = settings.JavaPath;
                if (IsCustomJava == null) IsCustomJava = settings.IsCustomJava;
                if (GamePath == null) GamePath = settings.GamePath;
                if (Xmx == null) Xmx = settings.Xmx;
                if (Xms == null) Xms = settings.Xms;
                if (WindowWidth == null) WindowWidth = settings.WindowWidth;
                if (WindowHeight == null) WindowHeight = settings.WindowHeight;
                if (IsShowConsole == null) IsShowConsole = settings.IsShowConsole;
                if (IsHiddenMode == null) IsHiddenMode = settings.IsHiddenMode;
                if (IsAutoUpdate == null) IsAutoUpdate = settings.IsAutoUpdate;
                if (GameArgs == null) GameArgs = settings.GameArgs;
                OnlineGameDirectConnection = settings.OnlineGameDirectConnection;
                LanguageId = settings.LanguageId;
            }
            else
            {
                if (settings.JavaPath != null) JavaPath = settings.JavaPath;
                if (settings.IsCustomJava != null) IsCustomJava = settings.IsCustomJava;
                if (settings.GamePath != null) GamePath = settings.GamePath;
                if (settings.Xmx != null) Xmx = settings.Xmx;
                if (settings.Xms != null) Xms = settings.Xms;
                if (settings.WindowWidth != null) WindowWidth = settings.WindowWidth;
                if (settings.WindowHeight != null) WindowHeight = settings.WindowHeight;
                if (settings.IsShowConsole != null) IsShowConsole = settings.IsShowConsole;
                if (settings.IsHiddenMode != null) IsHiddenMode = settings.IsHiddenMode;
                if (settings.IsAutoUpdate != null) IsAutoUpdate = settings.IsAutoUpdate;
                if (settings.GameArgs != null) GameArgs = settings.GameArgs;
                OnlineGameDirectConnection = settings.OnlineGameDirectConnection;
                LanguageId = settings.LanguageId;
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
            uint xmx = Environment.Is64BitOperatingSystem ? (uint)(NativeMethods.GetRamCount() / 2) : (uint)1024;
            return new Settings
            {
                JavaPath = "",
                IsCustomJava = false,
                GamePath = LaunсherSettings.gamePath,
                Xmx = xmx,
                Xms = 256,
                WindowWidth = 854,
                WindowHeight = 480,
                IsShowConsole = false,
                IsHiddenMode = false,
                GameArgs = "",
                IsAutoUpdate = false,
                OnlineGameDirectConnection = false
            };
        }
    }
}
